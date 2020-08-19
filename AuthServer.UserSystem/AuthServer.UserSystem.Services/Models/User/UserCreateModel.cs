using AuthServer.UserSystem.Domain.Entities;
using System.Collections.Generic;

namespace AuthServer.UserSystem.Services.Models
{
    public class CreateUserModel
    {
        private string _firstName;
        private string _lastName;
         
        public string AccountId { get; set; }
        public string FirstName { get { return _firstName; } set { _firstName = value.ToLower() ?? null; } }
        public string LastName { get { return _lastName; } set { _lastName = value.ToLower() ?? null; } }
        public Gender Gender { get; set; }
        public List<ContactModel> Contacts { get; set; }
        public List<string> RoleIds { get; set; } 
    }
}
