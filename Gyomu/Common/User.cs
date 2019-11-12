using System.Collections.Generic;
using System.Security.Principal;

namespace Gyomu.Common
{
    public abstract class User
    {
        public abstract List<string> GROUPS { get; }
        public abstract bool IsGroup { get; }

        public bool IsValid { get; protected set; }
        public abstract List<User> Members { get; }
        public string Region { get; protected set; }
        public string UserID { get; protected set; }

        public override bool Equals(object obj)
        {
            if (obj is User target)
            {
                return UserID.Equals(target.UserID);
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            if (UserID == null)
                return int.MinValue;
            return UserID.GetHashCode();
        }
        public abstract bool IsInGroup(User group_user);

        public static User CurrentUser
        {
            get
            {
                switch (System.Environment.OSVersion.Platform)
                {
                    case System.PlatformID.Win32NT:
                        return WindowsUser.CurrentWindowsUser;
                    default:
                        throw new System.InvalidOperationException("Not supported " + System.Environment.OSVersion.Platform.ToString());
                }
            }
        }

        public static User GetUser(IPrincipal principal)
        {
            switch (System.Environment.OSVersion.Platform)
            {
                case System.PlatformID.Win32NT:
                    return WindowsUser.GetWindowsUser(principal);
                default:
                    throw new System.InvalidOperationException("Not supported " + System.Environment.OSVersion.Platform.ToString());
            }
        }
        public static User GetUser(string userID)
        {
            switch (System.Environment.OSVersion.Platform)
            {
                case System.PlatformID.Win32NT:
                    return WindowsUser.GetWindowsUser(userID);
                default:
                    throw new System.InvalidOperationException("Not supported " + System.Environment.OSVersion.Platform.ToString());
            }
        }
    }
}