using act.core.web.Framework;

namespace act.core.web.Models.Home
{
    public class Welcome
    {
        public Welcome(IUserSecurity userSecurity)
        {
            FirstName = userSecurity.FirstName;
            LastName = userSecurity.LastName;
        }

        public string FirstName { get; }
        public string LastName { get; }
    }
}