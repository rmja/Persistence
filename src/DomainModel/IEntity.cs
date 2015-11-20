using System.Collections.Generic;

namespace DomainModel
{
    public interface IEntity
    {
		ICollection<IDomainEvent> Events { get; }
    }
}