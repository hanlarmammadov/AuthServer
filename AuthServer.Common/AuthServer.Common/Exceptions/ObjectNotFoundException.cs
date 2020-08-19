using System;

namespace AuthServer.Common.Exceptions
{
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException(string message) : base(message) { }
    }
}
