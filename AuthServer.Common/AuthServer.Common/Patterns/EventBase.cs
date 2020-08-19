using System;

namespace AuthServer.Common.Patterns
{
    public abstract class EventBase : MessageBase
    {
        public DateTime EventDate { get; set; }
        public string Issuer { get; set; }
        public string IssuerSystem { get; set; }
    }
}
