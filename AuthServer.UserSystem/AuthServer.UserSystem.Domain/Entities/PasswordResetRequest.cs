using System;

namespace AuthServer.UserSystem.Domain.Entities
{
    public class PasswordResetRequest
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime Created { get; set; }
        public bool IsResolved { get; set; }
    }
}
