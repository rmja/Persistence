using System;
using System.Threading.Tasks;

namespace Persistence
{
    public interface IUnitOfWork<TContext> : IDisposable
    {
		IRepository<TEntity> Repository<TEntity>() where TEntity : class;

		Task<int> CommitAsync();
    }
}