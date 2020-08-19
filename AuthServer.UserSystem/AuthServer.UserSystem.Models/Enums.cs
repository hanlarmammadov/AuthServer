
namespace AuthServer.UserSystem.Models
{ 
    public enum AccountValidationStatus
    {
        NotSet = 0,
        AccountInactive = 1,
        AccountBanned = 2,
        EmailNotConfirmed = 3,
        PasswordExpired = 4,
        PasswordIsTemporary = 5,
        Ok = 99
    } 
}
