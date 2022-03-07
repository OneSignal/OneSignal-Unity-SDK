using System;
using System.Collections.Generic;
using System.Linq;

namespace OneSignalSDK.Installer {
    public static class ReflectionHelpers {
        public static IEnumerable<Type> FindAllAssignableTypes<T>(string assemblyFilter) {
            var assignableType = typeof(T);
        
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var filteredAssemblies = assemblies.Where(assembly 
                => assembly.FullName.Contains(assemblyFilter));
        
            var allTypes = filteredAssemblies.SelectMany(assembly => assembly.GetTypes());
            var assignableTypes = allTypes.Where(type 
                => type != assignableType && assignableType.IsAssignableFrom(type));

            return assignableTypes;
        }
    }
}