using System;
using UnityEngine.Analytics;

namespace UnityEditor.VSAttribution.OneSignalSDK
{
	public static class VSAttribution
	{
		const int k_VersionId = 4;
		const int k_MaxEventsPerHour = 10;
		const int k_MaxNumberOfElements = 1000;

		const string k_VendorKey = "unity.vsp-attribution";
		const string k_EventName = "vspAttribution";

		static bool RegisterEvent()
		{
			AnalyticsResult result = EditorAnalytics.RegisterEventWithLimit(k_EventName, k_MaxEventsPerHour,
				k_MaxNumberOfElements, k_VendorKey, k_VersionId);

			var isResultOk = result == AnalyticsResult.Ok;
			return isResultOk;
		}

		[Serializable]
		struct VSAttributionData
		{
			public string actionName;
			public string partnerName;
			public string customerUid;
			public string extra;
		}

		/// <summary>
		/// Registers and attempts to send a Verified Solutions Attribution event.
		/// </summary>
		/// <param name="actionName">Name of the action, identifying a place this event was called from.</param>
		/// <param name="partnerName">Identifiable Verified Solutions Partner's name.</param>
		/// <param name="customerUid">Unique identifier of the customer using Partner's Verified Solution.</param>
		public static AnalyticsResult SendAttributionEvent(string actionName, string partnerName, string customerUid)
		{
			try
			{
				// Are Editor Analytics enabled ? (Preferences)
				if (!EditorAnalytics.enabled)
					return AnalyticsResult.AnalyticsDisabled;

				if (!RegisterEvent())
					return AnalyticsResult.InvalidData;

				// Create an expected data object
				var eventData = new VSAttributionData
				{
					actionName = actionName,
					partnerName = partnerName,
					customerUid = customerUid,
					extra = "{}"
				};

				return EditorAnalytics.SendEventWithLimit(k_EventName, eventData, k_VersionId);
			}
			catch
			{
				// Fail silently
				return AnalyticsResult.AnalyticsDisabled;
			}
		}
	}
}