using System.Collections.Generic;
using System.IO;
using OneSignalPush.Utilities;

/// <summary>
/// Representation of Manifest JSON file.
/// Can be used for adding dependencies, scopeRegistries, etc to .json file
/// </summary>
class Manifest
{
    const string k_ProjectManifestPath = "Packages/manifest.json";
    const string k_DependenciesKey = "dependencies";
    const string k_ScopedRegistriesKey = "scopedRegistries";

    /// <summary>
    /// Path to manifest file.
    /// </summary>
    public string Path { get; }

    readonly Dictionary<string, ScopeRegistry> m_ScopeRegistries;
    readonly Dictionary<string, Dependency> m_Dependencies;

    Dictionary<string, object> m_RawContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="Manifest"/> class.
    /// </summary>
    /// <param name="pathToFile">Path to manifest file.</param>
    public Manifest(string pathToFile = k_ProjectManifestPath)
    {
        Path = pathToFile;
        m_ScopeRegistries = new Dictionary<string, ScopeRegistry>();
        m_Dependencies = new Dictionary<string, Dependency>();
    }

    /// <summary>
    /// Read the Manifest file and deserialize its content from JSON.
    /// </summary>
    public void Fetch()
    {
        var manifestText = File.ReadAllText(Path);
        m_RawContent = (Dictionary<string, object>) Json.Deserialize(manifestText);

        if (m_RawContent.TryGetValue(k_ScopedRegistriesKey, out var registriesBlob))
        {
            if (registriesBlob is List<object> registries)
            {
                foreach (var registry in registries)
                {
                    var registryDict = (Dictionary<string, object>) registry;
                    var scopeRegistry = new ScopeRegistry(registryDict);
                    m_ScopeRegistries.Add(scopeRegistry.Url, scopeRegistry);
                }
            }
        }

        if (m_RawContent.TryGetValue(k_DependenciesKey, out var dependenciesBlob))
        {
            if (dependenciesBlob is Dictionary<string, object> dependencies)
            {
                foreach (var dependencyData in dependencies)
                {
                    var dependency = new Dependency(dependencyData.Key, dependencyData.Value.ToString());
                    m_Dependencies.Add(dependency.Name, dependency);
                }
            }
        }
    }

    /// <summary>
    /// Returns dependency by a provided name.
    /// </summary>
    /// <param name="name">Name of the dependency.</param>
    /// <returns>Dependency with given name.</returns>
    public Dependency GetDependency(string name)
    {
        return m_Dependencies[name];
    }

    /// <summary>
    /// Returns scope registry by a provided url.
    /// </summary>
    /// <param name="url">Scope registry url.</param>
    /// <returns>Scope registry with the given url.</returns>
    public ScopeRegistry GetScopeRegistry(string url)
    {
        return m_ScopeRegistries[url];
    }

    /// <summary>
    /// Adds scope registry.
    /// </summary>
    /// <param name="registry">An entry to add.</param>
    public void AddScopeRegistry(ScopeRegistry registry)
    {
        if (!IsRegistryPresent(registry.Url))
        {
            m_ScopeRegistries.Add(registry.Url, registry);
        }
    }
    
    /// <summary>
    /// Removes a scope registry
    /// </summary>
    /// <param name="url"></param>
    public void RemoveScopeRegistry(string url)
    {
        if (IsRegistryPresent(url))
        {
            m_ScopeRegistries.Remove(url);
        }
    }

    /// <summary>
    /// Adds dependency.
    /// </summary>
    /// <param name="name">Dependency name.</param>
    /// <param name="version">Dependency version.</param>
    public void AddDependency(string name, string version)
    {
        if (!IsDependencyPresent(name))
        {
            var dependency = new Dependency(name, version);
            m_Dependencies.Add(dependency.Name, dependency);
        }
    }
    
    /// <summary>
    /// Removes a dependency
    /// </summary>
    /// <param name="name"></param>
    public void RemoveDependency(string name)
    {
        if (IsDependencyPresent(name))
        {
            m_Dependencies.Remove(name);
        }
    }

    /// <summary>
    /// Writes changes back to the manifest file.
    /// </summary>
    public void ApplyChanges()
    {
        var registries = new List<object>();
        foreach (var registry in m_ScopeRegistries.Values)
        {
            registries.Add(registry.ToDictionary());
        }

        m_RawContent[k_ScopedRegistriesKey] = registries;

        // Remove 'scopedRegistries' key from raw content if we have zero scope registries.
        // Because we don't need an empty 'scopedRegistries' key in the manifest
        if (registries.Count == 0)
            m_RawContent.Remove(k_ScopedRegistriesKey);

        Dictionary<string, object> dependencies = new Dictionary<string, object>();
        foreach (var dependency in m_Dependencies.Values)
        {
            dependencies.Add(dependency.Name, dependency.Version);
        }

        m_RawContent[k_DependenciesKey] = dependencies;

        string manifestText = Json.Serialize(m_RawContent, true);
        File.WriteAllText(Path, manifestText);
    }

    /// <summary>
    /// Searches for ScopeRegistry with the provided Url.
    /// </summary>
    /// <param name="url">ScopeRegistry url to search for.</param>
    /// <returns>`true` if scoped registry found, `false` otherwise.</returns>
    public bool IsRegistryPresent(string url)
    {
        return m_ScopeRegistries.ContainsKey(url);
    }

    /// <summary>
    /// Searches for a specific dependency by the provided name.
    /// </summary>
    /// <param name="name">The dependency name to search for.</param>
    /// <returns>`true` if found, `false` otherwise.</returns>
    public bool IsDependencyPresent(string name)
    {
        return m_Dependencies.ContainsKey(name);
    }
}