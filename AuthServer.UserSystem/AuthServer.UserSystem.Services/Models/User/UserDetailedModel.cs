using AuthServer.UserSystem.Domain.Entities;
using System;
using System.Collections.Generic;

namespace AuthServer.UserSystem.Services.Models
{
    public class UserDetailedModel
    {
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public List<ContactModel> Contacts { get; set; }
        public List<RoleCreateModel> Roles { get; set; }

        public DateTime? CreateDate;
        public DateTime? ModifiedDate;
    }
}
