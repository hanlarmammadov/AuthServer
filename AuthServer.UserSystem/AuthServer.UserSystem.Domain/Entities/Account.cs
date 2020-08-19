using System;

namespace AuthServer.UserSystem.Domain.Entities
{
    public class Account
    {
        public string AccountId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; } 
        public PasswordStatus PasswordStatus { get; set; }
        public EmailStatus EmailStatus { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public AccountDataStatus AccountDataStatus { get; set; }
        public DateTime? PasswordLastChanged { get; set; }
        public DateTime AccountCreated { get; set; }
    }
}
