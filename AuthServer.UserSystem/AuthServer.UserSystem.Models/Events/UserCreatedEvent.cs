using AuthServer.Common.Patterns;
using System.Collections.Generic;

namespace AuthServer.UserSystem.Models.Events
{
    public class UserCreatedEvent : EventBase
    {
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> RoleIds { get; set; }
    }
}
