using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Persistence;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace System
{
    public static class ServiceProviderExtensions
    {
		public static IServiceProvider ConfigureEntities(this IServiceProvider serviceProvider, ModelBuilder modelBuilder)
		{
            var configurators = serviceProvider.GetServices<IEntityConfigurator>();
            
			foreach (var configurator in configurators)
			{
				var configuratorType = configurator.GetType();
				var genericConfiguratorInterface = configuratorType.GetInterfaces().Single(iface => iface.GetTypeInfo().IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityConfigurator<>));
				var modelType = genericConfiguratorInterface.GetGenericArguments()[0];

                if (modelBuilder.Model.FindEntityType(modelType) != null)
                {
                    var EntityPr = modelBuilder.GetType().GetMethod("Entity", new Type[] { }).MakeGenericMethod(modelType);
                    var ConfigureFn = configurator.GetType().GetMethod("Configure");

                    var entityBuilder = EntityPr.Invoke(modelBuilder, new object[] { });
                    ConfigureFn.Invoke(configurator, new[] { entityBuilder });
                }
			}

			return serviceProvider;
		}
    }
}