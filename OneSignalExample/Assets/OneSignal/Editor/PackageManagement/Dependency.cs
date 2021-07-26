using System.Collections.Generic;

/// <summary>
/// Representation of the manifest file "dependency" entry.
/// </summary>
class Dependency
{
    /// <summary>
    /// The dependency name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The dependency version.
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// Initializes a new instance of the  <see cref="Dependency"/> class with provided properties.
    /// </summary>
    /// <param name="name">Dependency name.</param>
    /// <param name="version">Dependency version.</param>
    public Dependency(string name, string version)
    {
        Name = name;
        Version = version;
    }

    /// <summary>
    /// Sets new dependency version.
    /// </summary>
    /// <param name="version">The version to be set for this dependency</param>
    public void SetVersion(string version)
    {
        Version = version;
    }

    /// <summary>
    /// Creates a dictionary from this object.
    /// </summary>
    /// <returns>Dependency object representation as Dictionary&lt;string, object&gt;.</returns>
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add(Name, Version);
        return result;
    }
}