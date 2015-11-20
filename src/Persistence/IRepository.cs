using System.Linq;

namespace Persistence
{
    public interface IRepository<TEntity> where TEntity : class
    {
		IQueryable<TEntity> Query();
		void Insert(TEntity entity);
		void Delete(TEntity entity);
    }
}