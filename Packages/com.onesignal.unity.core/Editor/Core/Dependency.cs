using System.Collections.Generic;

namespace OneSignalPush.Editor
{
    /// <summary>
    /// Representation of "dependencies" entries of manifest file
    /// </summary>
    class Dependency
    {
        /// <summary>
        /// Gets the Name of current dependency
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Version of current dependency
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Initializes a new instance of Dependency class with provided properties
        /// </summary>
        /// <param name="name">Name of new dependency</param>
        /// <param name="version">Version of new dependency</param>
        public Dependency(string name, string version)
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Sets new version of current dependency
        /// </summary>
        /// <param name="version">The version to be set for current dependency</param>
        public void SetVersion(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Creates dictionary from this object.
        /// </summary>
        /// <returns>Dependency object representation as Dictionary&lt;string, object&gt;.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string,object> result = new Dictionary<string, object>();
            result.Add(Name, Version);
            return result;
        }
    }
}
