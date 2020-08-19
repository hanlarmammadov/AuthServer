using System;

namespace AuthServer.Common.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message) : base(message) { }
    }
}
