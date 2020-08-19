
namespace AuthServer.UserSystem.Domain.Entities
{
    public enum PasswordStatus
    {
        Null = 0,
        Set = 1,
        Empty = 2,
        Expired = 3,
        Temp = 4
    }

    public enum EmailStatus
    {
        Null = 0,
        Confirmed = 1,
        NotConfirmed = 2,
    }

    public enum AccountStatus
    {
        Null = 0,
        Active = 1,
        Inactive = 2,
        Banned = 3
    }

    public enum AccountDataStatus
    {
        Null = 0,
        Completed = 1,
        NotCompleted = 2,
    }

    public enum Gender
    {
        NotSet = 0,
        Male = 1,
        Female = 2
    }

    public enum UserContactType
    {
        NotSet = 0,
        Email = 1,
        Mobile = 2
    }

    public enum EmailChangeRecordSatus
    {
        NotSet = 0,
        Changed = 1,
        RolledBack = 2
    }

    public enum ConfirmEmailRequestStatus
    {
        NotSet = 0,
        Resolved = 1,
        NotResolved = 2,
    }
}
