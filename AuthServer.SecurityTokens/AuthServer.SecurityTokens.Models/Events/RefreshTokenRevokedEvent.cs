using AuthServer.Common.Patterns;
using System;

namespace AuthServer.SecurityTokens.Models.Events
{
    public class RefreshTokenRevokedEvent : EventBase
    {
        public string TokenId { get; set; }
        public DateTime Expires { get; set; }
    }
}
