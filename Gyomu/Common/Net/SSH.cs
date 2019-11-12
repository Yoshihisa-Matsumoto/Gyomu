using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common.Net
{
    public class SSH : IDisposable
    {
        private Configurator Config { get; set; }
        private short ApplicationID { get; set; }
        private ConnectionInfo connInfo = null;
        private Models.RemoteConnectionInfo _connectionInformation = null;
        private SSH() { }

        public SSH(Models.RemoteConnectionInfo connectionInformation, short app_id, Configurator config)
        {
            Config = config;
            ApplicationID = app_id;
            _connectionInformation = connectionInformation;
            init();
        }
        private List<string> lstCertificate = new List<string>();
        private AuthenticationMethod AuthenticationMethod
        {
            get
            {
                if (_connectionInformation.Password == null)
                    return new PrivateKeyAuthenticationMethod(_connectionInformation.UserID, new PrivateKeyFile(_connectionInformation.PrivateKeyFileName));
                return new PasswordAuthenticationMethod(_connectionInformation.UserID, _connectionInformation.Password);
            }
        }
        private StatusCode init()
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;

            string serverURL = _connectionInformation.ServerURL;
            if (_connectionInformation.ProxyHost == null && _connectionInformation.ProxyPort == -1)
                connInfo = new ConnectionInfo(serverURL, _connectionInformation.Port, _connectionInformation.UserID, AuthenticationMethod);
            else
                connInfo = new ConnectionInfo(serverURL, _connectionInformation.Port, _connectionInformation.UserID, ProxyTypes.Http, _connectionInformation.ProxyHost, _connectionInformation.ProxyPort, _connectionInformation.ProxyUserID, _connectionInformation.ProxyPassword, AuthenticationMethod);

            if (string.IsNullOrEmpty(_connectionInformation.Password) == false)
            {
                if (lstCertificate.Contains(_connectionInformation.PrivateKeyFileName) == false)
                    lstCertificate.Add(_connectionInformation.PrivateKeyFileName);
            }
            return retVal;
        }

        private SshClient client = null;
        public StatusCode Open()
        {
            try
            {
                if (client != null)
                {
                    if (client.IsConnected)
                        return StatusCode.SUCCEED_STATUS;
                    else
                    {
                        client.Disconnect();
                        client = new SshClient(connInfo);
                        client.Connect();
                    }
                }
                else
                {
                    client = new SshClient(connInfo);
                    client.Connect();
                }
                return StatusCode.SUCCEED_STATUS;
            }
            catch (Exception ex)
            {
                return new CommonStatusCode(CommonStatusCode.SSH_CONNECT_ERROR, new object[] { connInfo.Host, connInfo.Username }, ex, Config, ApplicationID);
            }
        }

        public StatusCode Execute(string command, out string result, out string error, out int exitStatus, bool requireShell = true)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            result = null;
            error = null;
            exitStatus = -1;

            if (client == null || client.IsConnected == false)
            {
                //Console.WriteLine("Open SSH");
                retVal = Open();
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            try
            {
                string cmd = command;
                if (requireShell)
                    cmd = "source ~/.kshrc\n" + command;
                SshCommand cmd_result = client.RunCommand(cmd);
                result = cmd_result.Result;
                error = cmd_result.Error;
                exitStatus = cmd_result.ExitStatus;
                //if (exitStatus != 0)
                //{
                //    return new CommonStatusCode(CommonStatusCode.SSH_COMMAND_FAILED,new object[] { connInfo.Host, connInfo.Username, command,result,error,exitStatus }, _config, _app_id);
                //}
                return StatusCode.SUCCEED_STATUS;
            }
            catch (Exception ex)
            {
                return new CommonStatusCode(CommonStatusCode.SSH_COMMAND_ERROR, new object[] { connInfo.Host, connInfo.Username, command }, ex, Config, ApplicationID);
            }
        }

        public void Dispose()
        {
            try
            {
                foreach (string file in lstCertificate)
                {
                    if (file.ToUpper().EndsWith(".TMP") && System.IO.File.Exists(file))
                        System.IO.File.Delete(file);
                }
                if (client != null && client.IsConnected)
                {

                    client.Disconnect();

                }
            }
            catch (Exception)
            {

            }
        }
    }
}
