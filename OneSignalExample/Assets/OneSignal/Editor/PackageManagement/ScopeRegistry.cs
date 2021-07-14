using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Representation of "scopeRegistries" entry of the manifest file.
/// </summary>
class ScopeRegistry
{
    const string k_KeyName = "name";
    const string k_KeyUrl = "url";
    const string k_KeyScopes = "scopes";

    /// <summary>
    /// Registry name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Registry url.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Registry scopes.
    /// </summary>
    public HashSet<string> Scopes { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ScopeRegistry"/> class with the provided properties.
    /// </summary>
    /// <param name="name">Name of new scope registry.</param>
    /// <param name="url">Url of new scope registry.</param>
    /// <param name="scopes">Scopes of new scope registry.</param>
    public ScopeRegistry(string name, string url, HashSet<string> scopes)
    {
        Name = name;
        Url = url;
        Scopes = scopes;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ScopeRegistry"/> class with the provided data.
    /// </summary>
    /// <param name="dictionary">Data to fill this object. Must contain <see cref="k_KeyName">name</see>,
    /// <see cref="k_KeyUrl">url</see> and <see cref="k_KeyScopes">scopes</see>.</param>
    public ScopeRegistry(Dictionary<string, object> dictionary)
    {
        Name = (string) dictionary[k_KeyName];
        Url = (string) dictionary[k_KeyUrl];
        var scopes = (List<object>) dictionary[k_KeyScopes];
        Scopes = new HashSet<string>();
        foreach (var scope in scopes)
        {
            Scopes.Add((string) scope);
        }
    }

    /// <summary>
    /// Returns true if provided scope exists in current scope registry.
    /// </summary>
    /// <param name="scope">string scope to check if exists in this scope registry.</param>
    /// <returns>'true' if this ScopeRegistry contains scope, `false` otherwise.</returns>
    public bool HasScope(string scope)
    {
        return Scopes.Contains(scope);
    }

    /// <summary>
    /// Adds scope.
    /// </summary>
    /// <param name="scope">A scope to add.</param>
    public void AddScope(string scope)
    {
        if (!HasScope(scope))
            Scopes.Add(scope);
    }

    /// <summary>
    /// Generates a hash of this object data, excluding Name.
    /// </summary>
    /// <returns>Hash of this object.</returns>
    public override int GetHashCode()
    {
        int hash = 0;
        if (!string.IsNullOrEmpty(Url))
            hash ^= Url.GetHashCode();
        if (Scopes != null)
        {
            foreach (var scope in Scopes)
            {
                hash ^= scope.GetHashCode();
            }
        }

        return hash;
    }

    /// <summary>
    /// Method for matching entries, Name matching is not necessary.
    /// </summary>
    /// <param name="obj">Object to compare with.</param>
    /// <returns>'true' if url and scopes match, 'false' otherwise.</returns>
    public override bool Equals(object obj)
    {
        return obj is ScopeRegistry other &&
               Url == other.Url &&
               Scopes != null &&
               other.Scopes != null &&
               new HashSet<string>(Scopes).SetEquals(other.Scopes);
    }

    /// <summary>
    /// Creates dictionary from this object.
    /// </summary>
    /// <returns>ScopeRegistry object representation as Dictionary&lt;string, object&gt;.</returns>
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add(k_KeyName, Name);
        result.Add(k_KeyUrl, Url);
        result.Add(k_KeyScopes, Scopes.ToList());
        return result;
    }
}