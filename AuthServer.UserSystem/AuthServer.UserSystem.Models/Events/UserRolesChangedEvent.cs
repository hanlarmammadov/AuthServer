using AuthServer.Common.Patterns;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.UserSystem.Models.Events
{
    public class UserRolesChangedEvent : EventBase
    {
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> RoleIds { get; set; }
    }
}
