using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Principal;

namespace Gyomu.Common
{
    public class BaseConfigurator : Configurator
    {

        private string strMachineName;
        private IPAddress address;
        private int _instanceID;

        private static BaseConfigurator _config = null;
        User user = null;
        private BaseConfigurator()
        {
            user = User.CurrentUser;
            init();

        }
        private BaseConfigurator(IPrincipal principal)
        {
            user = User.GetUser(principal);
            webInit();
        }
        private bool _is_client = true;
        public static Configurator GetWebInstance(IPrincipal principal)
        {
            return new BaseConfigurator(principal);
        }
        public static Configurator GetWebInstance()
        {
            if (_config == null)
            {
                _config = new BaseConfigurator();
                _config.webInit();
            }
            return _config;
        }
        public static Configurator GetInstance()
        {
            if (_config == null)
                _config = new BaseConfigurator();
            return _config;
        }
        private void init()
        {
            strMachineName = System.Environment.MachineName;
            IPAddress[] hostEntry = System.Net.Dns.GetHostAddresses(strMachineName);
            address = hostEntry[0];
            _instanceID = System.Environment.TickCount;
        }
        private void webInit()
        {
            strMachineName = System.Environment.MachineName;
            IPAddress[] hostEntry = System.Net.Dns.GetHostAddresses(strMachineName);
            address = hostEntry[0];
            _instanceID = System.Diagnostics.Process.GetCurrentProcess().Id;
            _config._is_client = false;
        }
        public string MachineName
        {
            get { return strMachineName; }
        }

        public IPAddress Address
        {
            get { return address; }
        }

        public string Username
        {
            get { return user.UserID; }
        }

        public int UniqueInstanceIDPerMachine
        {
            get
            {
                if (_is_client)
                    return _instanceID;
                else
                    return System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
        }
        public string Region
        {
            get { return user.Region; }
        }

        public string Mode { get { return System.Environment.GetEnvironmentVariable(SettingItem.GYOMU_COMMON_MODE); } }

        #region IConfigurator Members


        public User User
        {
            get { return user; }
        }

        public short ApplicationID { get; set; }
        #endregion
    }
}
