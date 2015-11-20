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
		private readonly TContext _context;
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

		public UnitOfWork(TContext context, IServiceProvider serviceProvider)
		{
			_context = context;
			_serviceProvider = serviceProvider;
		}

		public async Task<int> CommitAsync()
		{
            var serviceProvider = _context.GetInfrastructure();
			var dispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();

			_context.ChangeTracker.DetectChanges();
			
			var domainEventEntities = _context.ChangeTracker.Entries<IEntity>()
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
				int count = await _context.SaveChangesAsync();

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
							_context);
					}

					_repositories.Add(type, instance);
				}
			}

			return (IRepository<T>)_repositories[type];
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}