using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Persistence
{
    public class DefaultDomainHandlerTypeProvider : IDomainHandlerTypeProvider
    {
        private static string _libraryName = "Persistence";
        private readonly ILibraryManager _libraryManager;

        public DefaultDomainHandlerTypeProvider(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public IEnumerable<TypeInfo> DomainHandlerTypes
        {
            get
            {
                var assemblies = GetCandidateLibraries()
                    .SelectMany(x => x.Assemblies)
                    .Select(Load);

                var types = assemblies.SelectMany(x => x.DefinedTypes);

                return types.Where(IsDomainHandler);
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

        private bool IsDomainHandler(TypeInfo typeInfo)
        {
            return typeof(IDomainHandler).GetTypeInfo().IsAssignableFrom(typeInfo) && typeInfo.IsClass;
        }
    }
}
