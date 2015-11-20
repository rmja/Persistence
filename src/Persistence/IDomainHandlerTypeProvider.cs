using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Persistence
{
    public interface IDomainHandlerTypeProvider
    {
        IEnumerable<TypeInfo> DomainHandlerTypes { get; }
    }
}
