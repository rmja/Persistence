using Microsoft.Data.Entity;
using System.Linq;

namespace Persistence
{
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
	{
		protected readonly DbContext _context;
		protected readonly DbSet<TEntity> _set;

		public Repository(DbContext context)
		{
			_context = context;
			_set = context.Set<TEntity>();
		}

		public IQueryable<TEntity> Query()
		{
			return _set;
		}

		public void Insert(TEntity entity)
		{
			_set.Add(entity);
		}

		public void Delete(TEntity entity)
		{
			_set.Remove(entity);
		}
	}
}