using System;

namespace AuthServer.Common.Exceptions
{
    public class TokenRevokedException : Exception
    {
        public TokenRevokedException(string message) : base(message) { }
    }
}
