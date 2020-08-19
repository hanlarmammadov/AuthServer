
namespace AuthServer.SecurityTokens.Models
{
    public class TokenAdditionalData
    {
        public string DeviceInfo { get; set; }
        public string RequesterIPv4 { get; set; }
        public string RequesterIPv6 { get; set; }
    }
}
