using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Update;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DomainModel;

namespace Persistence
{
	public class UnitOfWork<TContext> : IUnitOfWork<TContext>
		where TContext : DbContext, new()
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

        public TContext Context { get; private set; }

        public UnitOfWork(TContext context, IServiceProvider serviceProvider)
		{
			Context = context;
			_serviceProvider = serviceProvider;
		}

		public async Task<int> CommitAsync()
		{
            var serviceProvider = Context.GetInfrastructure();
			var dispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();

			Context.ChangeTracker.DetectChanges();
			
			var domainEventEntities = Context.ChangeTracker.Entries<IEntity>()
				.Select(x => x.Entity)
				.Where(x => x.Events.Any())
				.ToArray();

			foreach (var entity in domainEventEntities)
			{
				var events = entity.Events.ToArray();
				entity.Events.Clear();

				foreach (var domainEvent in events)
				{
					dispatcher.Dispatch(domainEvent);
				}
			}

			await dispatcher.DispatchPreCommitAsync<IUnitOfWork<TContext>>(this);

			try
			{
                int count = await Context.SaveChangesAsync();

				await dispatcher.DispatchPostCommitAsync<IUnitOfWork<TContext>>(this);

				return count;
			}
			catch (DbUpdateException e)
			{
				var sqlException = e.InnerException as SqlException;

				if (sqlException != null)
				{
					var errors = sqlException.Errors.OfType<SqlError>();

					if (errors.Any(x => x.Number == 2601 || x.Number == 2627))
					{
						// 2601: Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'. The duplicate key value is %ls.
						// 2627: Violation of %ls constraint '%.*ls'. Cannot insert duplicate key in object '%.*ls'. The duplicate key value is %ls.

						throw new UniqueConstraintException(e);
					}
                    else if (errors.Any(x => x.Number == 547))
                    {
                        // 547: The %ls statement conflicted with the %ls constraint "%.*ls". The conflict occurred in database "%.*ls", table "%.*ls"%ls%.*ls%ls.

                        throw new RelationException(e);
                    }
                }
                
				throw;
			}
		}

		public IRepository<T> Repository<T>() where T : class
		{
			var type = typeof(T);

			lock (_repositories)
			{
				if (!_repositories.ContainsKey(type))
				{
					object instance = _serviceProvider?.GetService(type);

					if (instance == null)
					{
						instance = Activator.CreateInstance(
							typeof(Repository<>).MakeGenericType(typeof(T)),
							Context);
					}

					_repositories.Add(type, instance);
				}
			}

			return (IRepository<T>)_repositories[type];
		}

		public void Dispose()
		{
			Context.Dispose();
		}
	}
}