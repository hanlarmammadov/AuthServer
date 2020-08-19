
namespace AuthServer.SecurityTokens.Entities
{
    public enum AccountRTokenStatus
    {
        NotSet = 0,
        Active = 1,
        Revoked = 2,
        ClaimsChanged = 3
    } 
}
