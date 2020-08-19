using System;

namespace AuthServer.SecurityTokens.Models
{
    public class TokenResult
    {
        public TokenResult(string jti, string token, DateTime expires)
        {
            Jti = jti;
            Token = token;
            Expires = expires;
        }

        public string Jti { get; set; }
        public DateTime Expires { get; set; }
        public string Token { get; set; }
    }
}
