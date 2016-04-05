using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.PlatformAbstractions;
using Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
		public static PersistenceServicesBuilder AddPersistence(this IServiceCollection services)
		{
			services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

            var typeProvider = new EntityConfiguratorTypeProvider();
            foreach (var typeInfo in typeProvider.EntityConfiguratorTypes)
            {
                services.AddSingleton(typeof(IEntityConfigurator), typeInfo.AsType());
            }

			return new PersistenceServicesBuilder(services.AddEntityFramework().AddSqlServer());
        }

        public static IServiceCollection AddDomainHandlers(this IServiceCollection services)
        {
            var typeProvider = new DomainHandlerTypeProvider();
            foreach (var typeInfo in typeProvider.DomainHandlerTypes)
            {
                var type = typeInfo.AsType();
                var eventHandlerGenericInterfaces = type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventDomainHandler<>));
                var commitHandlerGenericInterfaces = type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommitDomainHandler<>));

                services.AddScoped(type);
                foreach (var domainHandlerGenericInterface in eventHandlerGenericInterfaces)
                {
                    services.AddScoped(domainHandlerGenericInterface, serviceProvider => serviceProvider.GetService(type));
                }
                foreach (var commitHandlerGenericInterface in commitHandlerGenericInterfaces)
                {
                    services.AddScoped(commitHandlerGenericInterface, serviceProvider => serviceProvider.GetService(type));
                }
            }

            return services;
        }
    }

	public class PersistenceServicesBuilder
	{
		private readonly EntityFrameworkServicesBuilder _services;

		public PersistenceServicesBuilder(EntityFrameworkServicesBuilder services)
		{
			_services = services;
		}

		public PersistenceServicesBuilder AddUnitOfWork<TContext>(string connectionString)
			where TContext : DbContext
		{
			_services.GetInfrastructure().AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
                
			_services.AddDbContext<TContext>(options =>
			{
				options.UseSqlServer(connectionString);
			});

			return this;
		}

        public PersistenceServicesBuilder AddMultiTenantUnitOfWork<TContext>()
            where TContext : DbContext
        {
            _services.GetInfrastructure().AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

            _services.AddDbContext<TContext>();

            return this;
        }
    }
}