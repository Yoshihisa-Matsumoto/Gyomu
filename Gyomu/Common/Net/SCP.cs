using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gyomu.Common.Net
{
    public class SCP : IDisposable
    {
        private Gyomu.Common.Configurator Config { get; set; }
        private short ApplicationID;
        private Models.RemoteConnectionInfo _connectionInformation { get; set; }
        private ConnectionInfo connectionInformation = null;
        private SCP() { }

        public SCP(Models.RemoteConnectionInfo info, short app_id, Configurator config)
        {
            Config = config;
            ApplicationID = app_id;
            _connectionInformation = info;
            init();
        }
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
            {

                connectionInformation = new ConnectionInfo(serverURL, _connectionInformation.Port, _connectionInformation.UserID, AuthenticationMethod);
            }
            else
                connectionInformation = new ConnectionInfo(serverURL, _connectionInformation.Port, _connectionInformation.UserID, ProxyTypes.Http, _connectionInformation.ProxyHost, _connectionInformation.ProxyPort, _connectionInformation.ProxyUserID, _connectionInformation.ProxyPassword, AuthenticationMethod);

            return retVal;
        }

        private ScpClient client = null;
        public StatusCode Open()
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            try
            {
                if (client != null)
                {
                    if (client.IsConnected)
                        return StatusCode.SUCCEED_STATUS;
                    else
                    {
                        client.Disconnect();
                        client = new ScpClient(connectionInformation);
                        client.Connect();

                    }
                }
                else
                {
                    client = new ScpClient(connectionInformation);
                    client.Connect();
                }
                return StatusCode.SUCCEED_STATUS;
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SCP_CONNECT_ERROR, new object[] { connectionInformation.Host, connectionInformation.Username }, ex, Config, ApplicationID);
            }
        }

        public StatusCode Upload(Models.FileTransportInfo transportInformation)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;

            if (client == null || client.IsConnected == false)
            {
                retVal = Open();
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            try
            {
                if (transportInformation.SourceIsDirectory)
                    client.Upload(new System.IO.DirectoryInfo(transportInformation.SourceFullName), transportInformation.DestinationFolderName);
                else
                    client.Upload(new System.IO.FileInfo(transportInformation.SourceFullName), transportInformation.DestinationFolderName);
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SCP_UPLOAD_ERROR, new object[] { connectionInformation.Host, connectionInformation.Username, transportInformation.SourceFullName, transportInformation.DestinationFolderName }, ex, Config, ApplicationID);
            }
            return retVal;
        }
        public StatusCode Upload(MemoryStream ms, string destination)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;

            if (client == null || client.IsConnected == false)
            {
                retVal = Open();
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            try
            {
                client.Upload(ms, destination);

            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SCP_UPLOAD_ERROR, new object[] { connectionInformation.Host, connectionInformation.Username, "In Memory Stream", destination }, ex, Config, ApplicationID);
            }
            return retVal;
        }
        
        public StatusCode Download(Models.FileTransportInfo transportInformation)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;

            if (client == null || client.IsConnected == false)
            {
                retVal = Open();
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            try
            {
                if (transportInformation.SourceIsDirectory)
                {
                    DirectoryInfo directoryInformation = new DirectoryInfo(transportInformation.DestinationFullName);
                    if (directoryInformation.Exists)
                        directoryInformation.Delete();
                    client.Download(transportInformation.SourceFolderName, directoryInformation);
                }
                else
                {
                    FileInfo fileInformation = new FileInfo(transportInformation.DestinationFullName);
                    if (fileInformation.Exists)
                        fileInformation.Delete();
                    client.Download(transportInformation.SourceFullName, fileInformation);
                }
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SCP_DOWNLOAD_ERROR, new object[] { connectionInformation.Host, connectionInformation.Username, transportInformation.SourceFullName, transportInformation.DestinationFullName }, ex, Config, ApplicationID);
            }
            return retVal;
        }
        public StatusCode Download(string filename, out Stream st)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            st = new MemoryStream();
            if (client == null || client.IsConnected == false)
            {
                retVal = Open();
                if (retVal.IsSucceeded == false)
                    return retVal;
            }
            try
            {
                client.Download(filename, st);
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SCP_DOWNLOAD_ERROR, new object[] { connectionInformation.Host, connectionInformation.Username, filename, "In memory Stream" }, ex, Config, ApplicationID);
            }
            return retVal;
        }
        
        public void Dispose()
        {
            try
            {
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
