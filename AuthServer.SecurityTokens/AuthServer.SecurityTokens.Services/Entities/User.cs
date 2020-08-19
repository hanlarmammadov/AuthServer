using System.Collections.Generic;

namespace AuthServer.SecurityTokens.Services.Entities
{
    public class User
    {
        public string Username { get; set; } 
        public List<Role> Roles { get; set; }

        public User() { }

        public User(string username)
        {
            Username = username; 
            Roles = new List<Role>();
        } 

        public User AddRole(Role role)
        {
            Roles.Add(role);
            return this;
        }
    }
}
