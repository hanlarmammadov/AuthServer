using System;

namespace AuthServer.UserSystem.Domain.Entities
{
    public class EmailChangeRecord
    {
        public string RecordId { get; set; }
        public string AccountId { get; set; }
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public EmailChangeRecordSatus Status { get; set; }
    }
}
