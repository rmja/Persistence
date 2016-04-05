using Persistence;

namespace System.Reflection
{
    public static class TypeInfoExtensions
    {
        public static bool IsDomainHandler(this TypeInfo self)
        {
            return typeof(IDomainHandler).GetTypeInfo().IsAssignableFrom(self) && self.IsClass;
        }

        public static bool IsEntityConfigurator(this TypeInfo self)
        {
            return typeof(IEntityConfigurator).GetTypeInfo().IsAssignableFrom(self) && self.IsClass;
        }
    }
}
