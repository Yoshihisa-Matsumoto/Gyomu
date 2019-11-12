using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    public class RemoteConnectionInfo
    {
        public RemoteConnectionInfo()
        {
            IsPassive = false;
            SslEnabled = false;
            SslImplicit = true;
        }
        public string ServerURL { get; set; }
        public int Port { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string PrivateKeyFileName { get; set; }

        internal string ProxyHost = null;
        internal int ProxyPort = -1;
        internal string ProxyUserID = null;
        internal string ProxyPassword = null;
        public void SetProxy(string proxyHost, int port, string proxyUID, string proxyPWD)
        {
            ProxyHost = proxyHost; ProxyPort = port; ProxyUserID = proxyUID; ProxyPassword = proxyPWD;
        }

        public bool IsPassive { get; set; }
        internal bool SslEnabled { get; set; }
        internal bool SslImplicit { get; set; }
        public void SetSsl(bool isImplicit)
        {
            SslEnabled = true;
            SslImplicit = isImplicit;
        }
    }
}
