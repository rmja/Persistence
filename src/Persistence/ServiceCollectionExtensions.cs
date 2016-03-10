using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.PlatformAbstractions;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
		public static PersistenceServicesBuilder AddPersistence(this IServiceCollection services)
		{
            services.AddSingleton<IEntityConfiguratorTypeProvider, DefaultEntityConfiguratorTypeProvider>();
			services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

			return new PersistenceServicesBuilder(services.AddEntityFramework().AddSqlServer());
        }

        public static IServiceCollection AddDomainHandlers(this IServiceCollection services, ILibraryManager libraryManager)
        {
            var provider = new DefaultDomainHandlerTypeProvider(libraryManager);

            foreach (var typeInfo in provider.DomainHandlerTypes)
            {
                var type = typeInfo.AsType();
                var domainHandlerGenericInterfaces = type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IDomainHandler<>));
                var commitHandlerGenericInterface = type.GetInterfaces().SingleOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IDomainCommitHandler<>));

                services.AddScoped(type);
                foreach (var domainHandlerGenericInterface in domainHandlerGenericInterfaces)
                {
                    services.AddScoped(domainHandlerGenericInterface, serviceProvider =>
                    {
                        return serviceProvider.GetService(type);
                    });
                }
                if (commitHandlerGenericInterface != null)
                {
                    services.AddScoped(commitHandlerGenericInterface, serviceProvider =>
                    {
                        return serviceProvider.GetService(type);
                    });
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