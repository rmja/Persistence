using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace Persistence
{
    public class DomainHandlerTypeProvider
    {
        public IEnumerable<TypeInfo> DomainHandlerTypes
        {
            get
            {
                var assemblies = LoadCandidateAssemblies();
                var types = assemblies.SelectMany(x => x.DefinedTypes);

                return types.Where(x => x.IsDomainHandler());
            }
        }

        //private IEnumerable<RuntimeLibrary> GetCandidateLibraries()
        //{
        //    return DependencyContext.Default.RuntimeLibraries.Distinct();
        //}

        //private IEnumerable<Assembly> LoadCandidateAssemblies()
        //{
        //    return GetCandidateLibraries()
        //        .SelectMany(x => x.Assemblies)
        //        .Select(x => Assembly.Load(x.Name));
        //}

        private IEnumerable<Assembly> LoadCandidateAssemblies()
        {
            return DnxPlatformServices.Default.LibraryManager.GetReferencingLibraries("Persistence")
                .SelectMany(x => x.Assemblies)
                .Select(x => Assembly.Load(x));
        }
    }
}
