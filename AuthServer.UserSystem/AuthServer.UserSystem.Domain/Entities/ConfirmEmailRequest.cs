using System;

namespace AuthServer.UserSystem.Domain.Entities
{
    public class ConfirmEmailRequest
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime CreateDate { get; set; }
        public ConfirmEmailRequestStatus Status { get; set; }
        public DateTime? ResolveDate { get; set; }
    }
}
