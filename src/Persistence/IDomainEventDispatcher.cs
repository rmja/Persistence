using System;
using System.Threading.Tasks;
using DomainModel;

namespace Persistence
{
    public interface IDomainEventDispatcher
    {
		void Dispatch(IDomainEvent domainEvent);

		Task DispatchPreCommitAsync<TUnitOfWork>(TUnitOfWork uow);
		Task DispatchPostCommitAsync<TUnitOfWork>(TUnitOfWork uow);
	}
}