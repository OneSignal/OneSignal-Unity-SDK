using System;
using System.Collections.Generic;

namespace OneSignalSDK.User
{
    public sealed class LoginData
    {
        public string OneSignalId { get; }
        public string ExternalId { get; }
        public string EmailSubscriptionId { get; }
        public string SmsSubscriptionId { get; }

        internal LoginData(
            string onesignalId,
            string externalId,
            string emailSubscriptionId,
            string smsSubscriptionId
        )
        {
            OneSignalId = onesignalId;
            ExternalId = externalId;
            EmailSubscriptionId = emailSubscriptionId;
            SmsSubscriptionId = smsSubscriptionId;
        }
    }

    public sealed class OneSignalError
    {
        /// <summary>
        /// One of BACKEND_ERROR, INVALID_ARGUMENT, NOT_INITIALIZED, STORAGE_LOCKED, UNKNOWN.
        /// </summary>
        public string Code { get; }

        /// <summary>CLIENT or BACKEND.</summary>
        public string Source { get; }
        public string BackendCode { get; }
        public int? HttpStatus { get; }
        public string Message { get; }
        public IReadOnlyDictionary<string, object> Meta { get; }

        internal OneSignalError(
            string code,
            string source,
            string backendCode,
            int? httpStatus,
            string message,
            IReadOnlyDictionary<string, object> meta
        )
        {
            Code = code;
            Source = source;
            BackendCode = backendCode;
            HttpStatus = httpStatus;
            Message = message;
            Meta = meta;
        }
    }

    public sealed class LoginResult
    {
        internal const string UnsupportedPlatformMessage =
            "loginWithProfile is only supported on Android";

        public bool Success { get; }

        /// <summary>Null unless <see cref="Success"/> is true.</summary>
        public LoginData Data { get; }

        /// <summary>Empty on success, never empty on failure.</summary>
        public IReadOnlyList<OneSignalError> Errors { get; }

        private LoginResult(bool success, LoginData data, IReadOnlyList<OneSignalError> errors)
        {
            Success = success;
            Data = data;
            Errors = errors;
        }

        internal static LoginResult Failure(string code, string message) =>
            new LoginResult(
                false,
                null,
                new[] { new OneSignalError(code, "CLIENT", null, null, message, null) }
            );

        internal static LoginResult Unsupported() => Failure("UNKNOWN", UnsupportedPlatformMessage);

        /// <summary>Returns an INVALID_ARGUMENT failure, or null when the inputs are usable.</summary>
        internal static LoginResult ValidateInputs(string externalId, OneSignalUserProfile profile)
        {
            const string api = "loginWithProfile";
            if (InputGuard.IsMissing(externalId, api + ": externalId"))
                return Failure("INVALID_ARGUMENT", "externalId is required");
            if (InputGuard.IsNull(profile, api + ": profile"))
                return Failure("INVALID_ARGUMENT", "profile is required");
            if (
                profile.Tags != null
                && InputGuard.HasMissingEntries(profile.Tags, api + ": tags", true)
            )
                return Failure("INVALID_ARGUMENT", "tags must not contain null entries");
            if (
                profile.Aliases != null
                && InputGuard.HasMissingEntries(profile.Aliases, api + ": aliases", false)
            )
                return Failure("INVALID_ARGUMENT", "aliases must not contain empty entries");
            return null;
        }

        internal static LoginResult FromJson(string json)
        {
            Dictionary<string, object> root;
            try
            {
                root = Json.Deserialize(json) as Dictionary<string, object>;
            }
            catch (Exception)
            {
                root = null;
            }

            if (root == null || !(_get(root, "success") is bool success))
                return Failure("UNKNOWN", "Malformed loginWithProfile result");

            if (success)
            {
                var data = _parseData(_get(root, "data"));
                var strayErrors = _get(root, "error") is List<object> stray && stray.Count > 0;
                return data != null && !strayErrors
                    ? new LoginResult(true, data, Array.Empty<OneSignalError>())
                    : Failure("UNKNOWN", "Malformed loginWithProfile result data");
            }

            var errors = new List<OneSignalError>();
            if (_get(root, "error") is List<object> entries)
            {
                foreach (var entry in entries)
                    errors.Add(_parseError(entry));
            }

            return errors.Count > 0
                ? new LoginResult(false, null, errors)
                : Failure("UNKNOWN", "loginWithProfile failed without an error");
        }

        private static LoginData _parseData(object value)
        {
            if (
                !(value is Dictionary<string, object> data)
                || !(_get(data, "onesignalId") is string onesignalId)
                || !(_get(data, "externalId") is string externalId)
            )
                return null;

            return new LoginData(
                onesignalId,
                externalId,
                _get(data, "emailSubscriptionId") as string,
                _get(data, "smsSubscriptionId") as string
            );
        }

        private static OneSignalError _parseError(object value)
        {
            if (!(value is Dictionary<string, object> error))
                return new OneSignalError(
                    "UNKNOWN",
                    "CLIENT",
                    null,
                    null,
                    "Malformed loginWithProfile error",
                    null
                );

            int? httpStatus = null;
            switch (_get(error, "httpStatus"))
            {
                case long l when l >= int.MinValue && l <= int.MaxValue:
                    httpStatus = (int)l;
                    break;
                case double d when d >= int.MinValue && d <= int.MaxValue:
                    httpStatus = (int)d;
                    break;
            }

            return new OneSignalError(
                _get(error, "code") as string ?? "UNKNOWN",
                _get(error, "source") as string ?? "CLIENT",
                _get(error, "backendCode") as string,
                httpStatus,
                _get(error, "message") as string ?? "",
                _get(error, "meta") as Dictionary<string, object>
            );
        }

        private static object _get(Dictionary<string, object> source, string key) =>
            source.TryGetValue(key, out var value) ? value : null;
    }
}
