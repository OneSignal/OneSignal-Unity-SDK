using System.Collections.Generic;
using System.IO;
using OneSignalPush.MiniJSON;

namespace OneSignalPush.Editor
{
    /// <summary>
    /// Representation of Manifest JSON file.
    /// Can be used for adding dependencies, scopeRegistries to .json file
    /// </summary>
    class Manifest
    {
        const string k_ProjectManifestPath = "Packages/manifest.json";
        const string k_DependenciesKey = "dependencies";
        const string k_ScopedRegistriesKey = "scopedRegistries";

        /// <summary>
        /// Path to manifest file
        /// </summary>
        public string Path { get; }

        readonly Dictionary<string, ScopeRegistry> m_ScopeRegistries;
        readonly Dictionary<string, Dependency> m_Dependencies;

        Dictionary<string, object> m_RawContent;

        /// <summary>
        /// Initializes a new instance of ManifestModifier class.
        /// </summary>
        /// <param name="pathToFile">Path to manifest file. </param>
        public Manifest(string pathToFile = k_ProjectManifestPath)
        {
            Path = pathToFile;
            m_ScopeRegistries = new Dictionary<string, ScopeRegistry>();
            m_Dependencies = new Dictionary<string,Dependency>();
        }

        /// <summary>
        /// Read Manifest file and deserialize it's content from JSON
        /// </summary>
        public void Fetch()
        {
            var manifestText = File.ReadAllText(Path);
            m_RawContent = (Dictionary<string, object>)Json.Deserialize(manifestText);

            if (m_RawContent.TryGetValue(k_ScopedRegistriesKey, out var registriesBlob))
            {
                if (registriesBlob is List<object> registries)
                {
                    foreach (var registry in registries)
                    {
                        var registryDict = (Dictionary<string, object>)registry;
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
        /// Returns dependency by provided name
        /// </summary>
        /// <param name="name">Name to search for dependency</param>
        /// <returns>Dependency with given name</returns>
        public Dependency GetDependency(string name)
        {
            return m_Dependencies[name];
        }

        /// <summary>
        /// Returns scope registry by provided url
        /// </summary>
        /// <param name="url">Url to search for scope registry</param>
        /// <returns>Scope registry with given url</returns>
        public ScopeRegistry GetScopeRegistry(string url)
        {
            return m_ScopeRegistries[url];
        }

        /// <summary>
        /// Method for adding scope registries
        /// </summary>
        /// <param name="registry">Entry to add</param>
        public void AddScopeRegistry(ScopeRegistry registry)
        {
            if (!IsRegistryExists(registry.Url))
            {
                m_ScopeRegistries.Add(registry.Url, registry);
            }
        }

        /// <summary>
        /// Method for adding dependencies
        /// </summary>
        /// <param name="name">Name of dependency</param>
        /// <param name="version">Version of dependency</param>
        public void AddDependency(string name, string version)
        {
            if (!IsDependencyExists(name))
            {
                var dependency = new Dependency(name, version);
                m_Dependencies.Add(dependency.Name, dependency);
            }
        }

        /// <summary>
        /// Writes changes back to the manifest file
        /// </summary>
        public void ApplyChanges()
        {
            List<object> registries = new List<object>();
            foreach (var registry in m_ScopeRegistries.Values)
            {
                registries.Add(registry.ToDictionary());
            }
            m_RawContent[k_ScopedRegistriesKey] = registries;

            //Remove 'scopedRegistries' key from raw content if we have zero scope registries.
            //Because we don't need empty 'scopedRegistries' key in the manifest
            if (registries.Count == 0)
                m_RawContent.Remove(k_ScopedRegistriesKey);

            Dictionary<string,object> dependencies = new Dictionary<string, object>();
            foreach (var dependency in m_Dependencies.Values)
            {
                dependencies.Add(dependency.Name, dependency.Version);
            }
            m_RawContent[k_DependenciesKey] = dependencies;

            string manifestText = Json.Serialize(m_RawContent, true);
            File.WriteAllText(Path,manifestText);
        }

        /// <summary>
        /// Searches for ScopeRegistry with provided Url
        /// </summary>
        /// <param name="url">ScopeRegistry url to search for</param>
        /// <returns>True if scoped registry found, false otherwise</returns>
        public bool IsRegistryExists(string url)
        {
            return m_ScopeRegistries.ContainsKey(url);
        }

        /// <summary>
        /// Searches for specific dependency by provided name
        /// </summary>
        /// <param name="name">Dependency name to search for</param>
        /// <returns>True if found, false otherwise</returns>
        public bool IsDependencyExists(string name)
        {
            return m_Dependencies.ContainsKey(name);
        }
    }
}
