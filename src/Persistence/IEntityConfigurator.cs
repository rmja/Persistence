using Microsoft.Data.Entity.Metadata.Builders;

namespace Persistence
{
    public interface IEntityConfigurator<TEntity> : IEntityConfigurator
        where TEntity : class
    {
		void Configure(EntityTypeBuilder<TEntity> builder);
    }

	public interface IEntityConfigurator
	{
	}
}