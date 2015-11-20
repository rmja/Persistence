using System;

namespace Persistence
{
    public class UniqueConstraintException : Exception
    {
		public UniqueConstraintException(Exception innerException)
			: base("Cannot insert duplicate key row.", innerException)
		{
		}
	}
}