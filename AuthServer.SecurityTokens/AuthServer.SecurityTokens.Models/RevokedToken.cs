using System;

namespace AuthServer.SecurityTokens.Models
{
    public class RevokedTokenModel
    {
        public string TokenId { get; set; }
        public DateTime Expires { get; set; }

        public RevokedTokenModel(string tokenId, DateTime expires)
        {
            TokenId = tokenId;
            Expires = expires;
        }
    }
}
