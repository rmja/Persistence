using System.Threading.Tasks;
using DomainModel;

namespace Persistence
{
    public interface IEventDomainHandler<TEvent> : IDomainHandler
		where TEvent : IDomainEvent
    {
		void Handle(TEvent domainEvent);
	}

    public interface ICommitDomainHandler<TUnitOfWork> : IDomainHandler
    {
        Task PreCommitAsync(TUnitOfWork uow);
        Task PostCommitAsync(TUnitOfWork uow);
    }

    public interface IDomainHandler
    {

    }
}