using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DomainModel;

namespace Persistence
{
	public class DomainEventDispatcher : IDomainEventDispatcher
	{
		private readonly IServiceProvider _serviceProvider;

		public DomainEventDispatcher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Dispatch(IDomainEvent domainEvent)
		{
			var eventType = domainEvent.GetType();
			var handlerType = typeof(IDomainHandler<>).MakeGenericType(eventType);
			var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
			var handlers = (IEnumerable)_serviceProvider.GetService(handlersType);

			if (handlers != null)
			{
				foreach (var handler in handlers)
				{
					var HandleFn = handler.GetType().GetMethod("Handle", new Type[] { eventType });

					HandleFn.Invoke(handler, new object[] { domainEvent });
				}
			}
		}
		public async Task DispatchPreCommitAsync<TUnitOfWork>(TUnitOfWork uow)
		{
			var handlers = _serviceProvider.GetService<IEnumerable<IDomainCommitHandler<TUnitOfWork>>>();

			if (handlers != null)
			{
				foreach (var handler in handlers)
				{
					await handler.PreCommitAsync(uow);
				}
			}
		}

		public async Task DispatchPostCommitAsync<TUnitOfWork>(TUnitOfWork uow)
		{
			var handlers = _serviceProvider.GetService<IEnumerable<IDomainCommitHandler<TUnitOfWork>>>();

			if (handlers != null)
			{
				foreach (var handler in handlers)
				{
					await handler.PostCommitAsync(uow);
				}
			}
		}
	}
}