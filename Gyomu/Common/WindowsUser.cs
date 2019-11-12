using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.DirectoryServices;

namespace Gyomu.Common
{
    public class WindowsUser : User
    {
        private string _display_name = null;
        public string DisplayName
        {
            get
            {
                if (_is_mail_displayname_initialized == false)
                    initMail();
                return _display_name;
            }
        }

        private List<string> groups = new List<string>();
        public override List<string> GROUPS
        {
            get
            {
                if (_is_group_initilized == false)
                    initGroup();
                return groups;
            }
        }


        

        public string SID { get; private set; }

        private WindowsIdentity _identity = null;

        private DirectoryEntry ntUser;
        private DirectoryEntry adUser;
        public List<string> OU_List { get; private set; }

        private string _mail_address = null;
        public string MailAddress
        {
            get
            {
                if (_is_mail_displayname_initialized == false)
                    initMail();
                return _mail_address;
            }
        }

        

        private User _manager = null;
        private bool _manager_retrieval_tried = false;
        public User Manager
        {
            get
            {
                if (_manager != null)
                    return _manager;
                if (_manager_retrieval_tried == false)
                    retrieveManager();
                return _manager;

            }
        }

        private bool _isGroup = false;
        public override bool IsGroup
        {
            get
            {
                if (_is_initilized_group_account == false)
                    initGroupAccount();
                return _isGroup;
            }
        }

        private List<User> _lstMembers = new List<User>();
        public override List<User> Members
        {
            get
            {
                if (IsGroup)
                {
                    if (_is_initilized_group_member == false)
                        initGroupMember();
                    return _lstMembers;
                }
                return _lstMembers;
            }
        }

        private string _distinguishedName = null;
        private string DistinguishedName
        {
            get
            {
                if (_distinguishedName == null)
                {
                    _distinguishedName = adUser.Properties["distinguishedName"].Value.ToString();
                }
                return _distinguishedName;
            }
        }

        private static Dictionary<string, WindowsUser> dictUser = new Dictionary<string, WindowsUser>();
        private static Dictionary<string, WindowsUser> dictDistinguishedUser = new Dictionary<string, WindowsUser>();

        private static void getUserList(string searchFilter, ref List<User> lstUser)
        {
            DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(null))
            {
                Filter = searchFilter
            };
            using (SearchResultCollection collection = ds.FindAll())
            {
                foreach (SearchResult sr in collection)
                {
                    lock (dictDistinguishedUser)
                    {
                        string distinguishedName = null;
                        if (sr.Properties.Contains("distinguishedName"))
                        {
                            distinguishedName = sr.Properties["distinguishedName"][0].ToString();
                            if (dictDistinguishedUser.ContainsKey(distinguishedName))
                                continue;

                        }
                        WindowsUser user = new WindowsUser(sr.GetDirectoryEntry());
                        if (user.IsValid)
                        {
                            if (string.IsNullOrEmpty(distinguishedName) == false)
                                dictDistinguishedUser.Add(distinguishedName, user);
                            lstUser.Add(user);
                        }
                    }
                }
            }

        }
        public static User GetUserFromEmailAddress(string emailAddress)
        {
            DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(null))
            {
                Filter = "mail=" + emailAddress
            };
            SearchResult sr = ds.FindOne();

            DirectoryEntry adUser = sr.GetDirectoryEntry();
            return new WindowsUser(adUser.Properties["sAMAccountName"].Value.ToString());

        }
        public static WindowsUser GetWindowsUser(string user_account_name_without_domain)
        {
            lock (dictUser)
            {
                if (dictUser.ContainsKey(user_account_name_without_domain))
                    return dictUser[user_account_name_without_domain];
                return new WindowsUser(user_account_name_without_domain);
            }
        }
        public static User GetWindowsUser(IPrincipal principal)
        {
            string account = principal.Identity.Name;
            if (string.IsNullOrEmpty(account))
                return null;
            string[] parts = account.Split('\\');
            if (parts.Length > 1)
                account = parts[parts.Length - 1];
            return new WindowsUser(account);
        }
        private WindowsUser(string user_account_name_without_domain)
        {
            string name = null;
            try
            {
                name = retrieveValidAccountName(user_account_name_without_domain);
                WindowsIdentity identity = null;
                identity = new WindowsIdentity(name);
                init(identity);
            }
            catch (Exception)
            {
                try
                {
                    adUser = new DirectoryEntry("WinNT://"+ System.Environment.GetEnvironmentVariable("USERDOMAIN") + "/" + name);
                    DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(null))
                    {
                        Filter = "samaccountname=" + adUser.Properties["Name"].Value.ToString()
                    };
                    SearchResult sr = ds.FindOne();

                    adUser = sr.GetDirectoryEntry();
                    UserID = adUser.Properties["sAMAccountName"].Value.ToString();
                    initAdUser();
                }
                catch (Exception )
                {

                }
            }
            if (IsValid && dictUser.ContainsKey(UserID) == false)
            {
                dictUser.Add(UserID, this);
            }
        }

        private WindowsUser(DirectoryEntry ad_user)
        {
            adUser = ad_user;
            if (adUser == null || adUser.Properties.Contains("sAMAccountName") == false)
                return;
            UserID = adUser.Properties["sAMAccountName"].Value.ToString();
            initAdUser();
            if (IsValid && dictUser.ContainsKey(UserID) == false)
            {
                dictUser.Add(UserID, this);
            }
        }
        private WindowsUser()
        {
            try
            {
                WindowsIdentity identity = null;
                identity = WindowsIdentity.GetCurrent();
                init(identity);
            }
            catch (Exception)
            {
                IsValid = false;
            }
        }

        public static void SetCurrentUserID(string uid)
        {
            if (_current.IsValid == false)
            {
                _current.UserID = uid;
                _current.IsValid = true;
            }
        }

        private void init(WindowsIdentity identity)
        {
            if (identity == null)
                return;

            OU_List = new List<string>();

            UserID = CutDomain(identity.Name);
            _identity = identity;


            ntUser = new DirectoryEntry("WinNT://" + identity.Name.Replace("\\", "/"));


            DirectorySearcher ds = new DirectorySearcher(new DirectoryEntry(null))
            {
                Filter = "samaccountname=" + identity.Name.Split('\\')[1]
            };
            SearchResult sr = ds.FindOne();
            adUser = sr.GetDirectoryEntry();

            SID = identity.User.Value;

            initAdUser();


            if (IsValid && dictUser.ContainsKey(UserID) == false)
            {
                dictUser.Add(UserID, this);
            }
        }
        private bool _is_group_initilized = false;

        private void initGroup()
        {
            if (_identity != null)
            {
                IdentityReferenceCollection colGroups = _identity.Groups;
                foreach (IdentityReference group in colGroups)
                {
                    try
                    {
                        NTAccount account = (NTAccount)group.Translate(typeof(NTAccount));
                        string group_name = CutDomain(account.ToString());
                        if (groups.Contains(group_name) == false)
                            groups.Add(group_name);
                    }
                    catch (Exception) { }
                }
            }
            try
            {
                int propertyCount = adUser.Properties["memberOf"].Count;
                String dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount;
                     propertyCounter++)
                {
                    dn = (String)adUser.Properties["memberOf"][propertyCounter];

                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        continue;
                    }
                    string group_name = dn.Substring((equalsIndex + 1),
                                      (commaIndex - equalsIndex) - 1);
                    if (groups.Contains(group_name) == false)
                        groups.Add(group_name);


                }
            }
            catch (Exception) { }
            _is_group_initilized = true;
        }
        private string decideRegion()
        {
            return null;
        }
        private void initAdUser()
        {

            OU_List = new List<string>();

            string distinguishedName = adUser.Properties["distinguishedName"].Value.ToString();
            string[] items = distinguishedName.Split(',');
            foreach (string item in items)
            {
                if (item.ToUpper().StartsWith("OU="))
                {
                    OU_List.Add(item.Substring(3));
                }
            }

            Region=decideRegion();

            IsValid = true;
        }

        private bool _is_initilized_group_account = false;
        private bool _is_initilized_group_member = false;
        private void initGroupMember()
        {
            if (_isGroup)
            {
                _lstMembers.Clear();

                if (adUser.Properties.Contains("member"))
                {
                    object[] properties = (object[])adUser.Properties["member"].Value;

                    lock (dictDistinguishedUser)
                    {
                        foreach (object obj in properties)
                        {
                            string member_distinguishedName = obj.ToString();

                            if (dictDistinguishedUser.ContainsKey(member_distinguishedName))
                            {
                                _lstMembers.Add(dictDistinguishedUser[member_distinguishedName]);
                                continue;
                            }

                            DirectoryEntry adMember = new DirectoryEntry("LDAP://" + member_distinguishedName);
                            WindowsUser member = new WindowsUser(adMember);
                            if (member.IsValid)
                            {
                                _lstMembers.Add(member);
                                dictDistinguishedUser.Add(member_distinguishedName, member);
                            }

                        }

                    }



                }
            }
            _is_initilized_group_member = true;
        }
        private void initGroupAccount()
        {
            try
            {
                if (adUser.Properties.Contains("groupType"))
                    _isGroup = true;
            }
            catch (Exception) { }

            _is_initilized_group_account = true;

        }

        private bool _is_mail_displayname_initialized = false;
        private void initMail()
        {
            if (adUser.Properties.Contains("mail"))
                _mail_address = adUser.Properties["mail"].Value.ToString();

            if (adUser.Properties.Contains("displayName"))
                _display_name = adUser.Properties["displayName"].Value.ToString();
            else if (adUser.Properties.Contains("name"))
                _display_name = adUser.Properties["name"].Value.ToString();

            _is_mail_displayname_initialized = true;
        }

        private static WindowsUser _current = new WindowsUser();
        public static WindowsUser CurrentWindowsUser
        {
            get
            {
                return _current;
            }
        }

        public static string CutDomain(string val)
        {
            if (val.IndexOf(DOMAIN_SEPARATOR)>=0)
                return val.Split(DOMAIN_SEPARATOR)[1];

            return val;
        }

        private const char DOMAIN_SEPARATOR =  '\\' ;
        private const char MAIL_SEPARATOR = '@' ;
        private static string retrieveValidAccountName(string value)
        {

            string name = CutDomain(value);
            if (name.Contains("\\"))
                name = name.Split(DOMAIN_SEPARATOR)[1];
            if (name.Contains("@"))
                name = name.Split(MAIL_SEPARATOR)[0];
            return name;

        }

        private void retrieveManager()
        {
            try
            {
                if (adUser != null)
                {
                    if (adUser.Properties.Contains("manager"))
                    {
                        string distinguishedName = adUser.Properties["manager"].Value.ToString();
                        DirectoryEntry adManager = new DirectoryEntry("LDAP://" + distinguishedName);
                        if (adManager.Properties.Contains("sAMAccountName"))
                        {
                            _manager = new WindowsUser(adManager.Properties["sAMAccountName"].Value.ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                _manager_retrieval_tried = true;
            }
        }
        public string DumpAllADFields()
        {
            if (adUser == null)
                return "";

            StringBuilder strBuf = new StringBuilder();
            foreach (string name in adUser.Properties.PropertyNames)
            {
                strBuf.AppendLine(name + ":");
                PropertyValueCollection pvc = adUser.Properties[name];
                bool first = true;
                foreach (object obj in pvc)
                {
                    if (first) first = false;
                    else strBuf.Append(",");

                    string str = obj.ToString();
                    if (str == "System.Byte[]")
                    {
                        System.Byte[] bytes = (System.Byte[])obj;
                        strBuf.Append("0x");
                        foreach (byte b in bytes)
                        {
                            strBuf.Append(b.ToString("X"));
                        }
                    }
                    else if (str == "System.__ComObject")
                    {
                        Type t = obj.GetType();
                    }
                    else
                    {
                        strBuf.Append(str);
                    }
                    strBuf.AppendLine();
                }
                strBuf.AppendLine();
            }
            strBuf.AppendLine();
            return strBuf.ToString();
        }
        

        public override bool IsInGroup(User group_user)
        {
            if (group_user.IsValid == false)
                return false;
            if (_identity != null)
            {
                WindowsPrincipal p = new WindowsPrincipal(_identity);
                return p.IsInRole(group_user.UserID);
            }
            if (group_user.IsGroup == false)
                return false;

            List<string> lstLikelyGroup = new List<string>();
            foreach (User member in group_user.Members)
            {
                if (member.IsValid && member.Equals(this))
                    return true;

                if (member.IsGroup)
                {
                    bool result = IsInGroup(member);
                    if (result)
                        return true;
                }
            }

 
            return false;
        }

    }
}
