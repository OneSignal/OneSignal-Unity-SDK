/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections.Generic;

namespace OneSignalSDK {
    /// <summary>
    /// Representation of the manifest file "dependency" entry.
    /// </summary>
    public class Dependency {
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
        public Dependency(string name, string version) {
            Name    = name;
            Version = version;
        }

        /// <summary>
        /// Sets new dependency version.
        /// </summary>
        /// <param name="version">The version to be set for this dependency</param>
        public void SetVersion(string version) {
            Version = version;
        }

        /// <summary>
        /// Creates a dictionary from this object.
        /// </summary>
        /// <returns>Dependency object representation as Dictionary&lt;string, object&gt;.</returns>
        public Dictionary<string, object> ToDictionary() {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add(Name, Version);

            return result;
        }
    }
}