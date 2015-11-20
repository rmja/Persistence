using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Persistence
{
    public class DefaultEntityConfiguratorTypeProvider : IEntityConfiguratorTypeProvider
    {
        private static string _libraryName = "Persistence";
        private readonly ILibraryManager _libraryManager;

        public DefaultEntityConfiguratorTypeProvider(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public IEnumerable<TypeInfo> EntityConfiguratorTypes
        {
            get
            {
                var assemblies = GetCandidateLibraries()
                    .SelectMany(x => x.Assemblies)
                    .Select(Load);

                var types = assemblies.SelectMany(x => x.DefinedTypes);

                return types.Where(IsEntityConfigurator);
            }
        }

        private IEnumerable<Library> GetCandidateLibraries()
        {
            return _libraryManager.GetReferencingLibraries(_libraryName)
                .Distinct()
                .Where(IsCandidateLibrary);
        }

        private static Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }

        private bool IsCandidateLibrary(Library library)
        {
            return library.Name != _libraryName;
        }

        private bool IsEntityConfigurator(TypeInfo typeInfo)
        {
            return typeof(IEntityConfigurator).GetTypeInfo().IsAssignableFrom(typeInfo) && typeInfo.IsClass;
        }
    }
}
