using AuthServer.Common.Patterns;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AuthServer.UserSystem.Models.MQ
{
    public class UserClaimsMQResponse : MessageBase
    {
        public bool OpSuccess { get; set; }
        public List<ClaimModel> Claims { get; set; }

        public void SetClaims(List<Claim> claims)
        {
            Claims = claims.Select(c => new ClaimModel(c.Type, c.Value)).ToList();
        }

        public List<Claim> GetClaims()
        {
            if (Claims == null)
                Claims = new List<ClaimModel>();
            return Claims.Select(cm => new Claim(cm.Type, cm.Value)).ToList();
        }
    }
}
