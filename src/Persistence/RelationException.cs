using System;

namespace Persistence
{
    public class RelationException : Exception
    {
		public RelationException(Exception innerException)
			: base("Relation exception.", innerException)
		{
		}
	}
}