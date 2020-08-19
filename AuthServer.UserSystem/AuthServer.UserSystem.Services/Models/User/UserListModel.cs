
using AuthServer.UserSystem.Domain.Entities;

namespace AuthServer.UserSystem.Services.Models
{
    public class UserListModel
    {
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; } 
    }
}
