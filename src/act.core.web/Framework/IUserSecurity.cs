
namespace act.core.web.Framework
{
    public interface IUserSecurity
    {
        string UserName { get; set; }
        string SamAccountName { get; set; }
        string Email { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        long EmployeeId { get; set; }

        // public UserSecurity()
        // {
        //     UserName = "Priyanka Kovermanne Somashekhara";
        //     SamAccountName = "kovpri01";
        //     Email = "Priyanka.KovermanneSomashekhara@csgi/com";
        //     FirstName = "Priyanka";
        //     LastName = "Kovermanne Somashekhara";
        //     EmployeeId = 81896;
        //  }
    }
}