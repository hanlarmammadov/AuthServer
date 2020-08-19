using System;
using System.Collections.Generic;

namespace AuthServer.UserSystem.Domain.Entities
{
    public class User
    {
        public string AccountId { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public List<Contact> Contacts { get; set; }
        public List<string> RoleIds { get; set; }

        public DateTime? CreateDate;
        public DateTime? ModifiedDate;
    }
}
