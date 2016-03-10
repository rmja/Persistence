using System;

namespace Persistence
{
    public class RelationException : Exception
    {
        public RelationException(string message)
            : base(message)
        {

        }

		public RelationException(Exception innerException)
			: base("Relation exception.", innerException)
		{
		}
	}
}