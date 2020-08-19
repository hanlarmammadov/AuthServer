using System;

namespace AuthServer.SecurityTokens.Entities
{
    public class AccountRTokenInfo
    {
        public string TokenId { get; set; }
        public string AccountId { get; set; }
        public AccountRTokenStatus Status { get; set; }
        public string DeviceInfo { get; set; }
        public string RequesterIPv4 { get; set; }
        public string RequesterIPv6 { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
