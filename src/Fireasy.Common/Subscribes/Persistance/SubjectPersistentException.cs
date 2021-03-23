using System;

namespace Fireasy.Common.Subscribes.Persistance
{
    public class SubjectPersistentException : Exception
    {
        public SubjectPersistentException(string message, Exception exception)
            : base (message, exception)
        {
        }
    }
}
