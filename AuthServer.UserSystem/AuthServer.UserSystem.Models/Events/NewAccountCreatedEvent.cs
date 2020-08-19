using AuthServer.Common.Patterns;
using System;

namespace AuthServer.UserSystem.Models.Events
{
    public class NewAccountCreatedEvent: EventBase
    {
        public string AccountId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
