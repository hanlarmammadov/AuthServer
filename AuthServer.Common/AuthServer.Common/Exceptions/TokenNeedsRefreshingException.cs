using System;

namespace AuthServer.Common.Exceptions
{
    public class TokenNeedsRefreshingException : Exception
    {
        public TokenNeedsRefreshingException(string message) : base(message) { }
    }
}
