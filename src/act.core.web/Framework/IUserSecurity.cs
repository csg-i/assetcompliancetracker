
namespace act.core.web.Framework
{
    public interface IUserSecurity
    {
        string UserName { get; }
        string SamAccountName { get; }
        string Email { get; }
        string FirstName { get; }
        string LastName { get; }
        long EmployeeId { get; }
    }
}