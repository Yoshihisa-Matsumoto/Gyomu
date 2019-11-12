using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Renci.SshNet;

namespace Gyomu.Common.Net
{
    public class SFTP : IDisposable
    {
        private Gyomu.Common.Configurator Config { get; set; }
        private short ApplicationID { get; set; }

        private ConnectionInfo connectionInformation = null;
        private Models.RemoteConnectionInfo _connectionInformation = null;

        private SFTP() { }

        public SFTP(short app_id, Configurator config, Models.RemoteConnectionInfo connectionInformation)
        {
            Config = config;
            ApplicationID = app_id;
            _connectionInformation = connectionInformation;
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

        public StatusCode GetModifiedDateSize(Models.FileTransportInfo transportInformation, out DateTime updateTime, out long lSize)
        {
            lSize = -1;
            updateTime = DateTime.Today;
            StatusCode retVal = init();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;
            SftpClient client = null;
            try
            {
                using (client = new SftpClient(connectionInformation))
                {
                    client.Connect();
                    Renci.SshNet.Sftp.SftpFileAttributes attr = client.GetAttributes(transportInformation.SourceFullName);
                    lSize = attr.Size;
                    updateTime = attr.LastWriteTime;

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_WRITETIME_ERROR, new object[] { connectionInformation.Host, transportInformation.SourceFolderName, transportInformation.SourceFileName }, ex, Config, ApplicationID);
            }
            finally
            {

            }
            return retVal;
        }
        public StatusCode Exists(Models.FileTransportInfo transportInformation, out bool exist)
        {
            StatusCode retVal = init();
            exist = false;
            SftpClient client = null;
            try
            {
                using (client = new SftpClient(connectionInformation))
                {
                    client.Connect();
                    exist = client.Exists(transportInformation.SourceFullName);
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_LISTUP_ERROR, new object[] { connectionInformation.Host, transportInformation.SourceFolderName }, ex, Config, ApplicationID);
            }
            finally
            {

            }
            return StatusCode.SUCCEED_STATUS;
        }
        public StatusCode ListFiles(Models.FileTransportInfo transportInformation, out List<string> lstFile)
        {
            StatusCode retVal = init();
            lstFile = new List<string>();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;
            SftpClient client = null;
            try
            {
                using (client = new SftpClient(connectionInformation))
                {
                    client.Connect();
                    foreach (Renci.SshNet.Sftp.SftpFile file in client.ListDirectory(transportInformation.SourceFolderName))
                    {
                        if (transportInformation.SourceFileName == null || transportInformation.SourceFileName.Length == 0)
                            lstFile.Add(file.Name);
                        else
                        {
                            if (Regex.IsMatch(file.Name, transportInformation.SourceFileName))
                            {
                                lstFile.Add(file.Name);
                                break;
                            }


                        }
                    }
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_LISTUP_ERROR, new object[] { connectionInformation.Host, transportInformation.SourceFolderName }, ex, Config, ApplicationID);
            }
            finally
            {

            }
            return StatusCode.SUCCEED_STATUS;
        }
        public StatusCode Upload(Models.FileTransportInfo transportInformation, bool canOverride)
        {
            StatusCode retVal = init();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;
            SftpClient client = null;
            try
            {
                using (client = new SftpClient(connectionInformation))
                {
                    client.Connect();

                    using (System.IO.FileStream fs = new System.IO.FileStream(transportInformation.SourceFullName, System.IO.FileMode.Open))
                    {
                        client.UploadFile(fs, transportInformation.DestinationFullName, canOverride);

                    }
                    client.Disconnect();
                }
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException spnex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_FILE_NOT_FOUND, new object[] { connectionInformation.Host, transportInformation.DestinationFolderName, transportInformation.DestinationFileName }, spnex, Config,ApplicationID);
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_UPLOAD_ERROR, new object[] { connectionInformation.Host, transportInformation.DestinationFolderName, transportInformation.SourceFullName }, ex, Config,ApplicationID);
            }
            finally
            {

            }

            return retVal;
        }
        SftpClient streamingClient = null;
        public void Dispose()
        {
            try
            {
                if (streamingClient != null && streamingClient.IsConnected)
                {
                    streamingClient.Disconnect();
                }
            }
            catch (Exception)
            {

            }

        }
        public StatusCode Download(Models.FileTransportInfo transportInformation, out System.IO.Stream st)
        {
            st = null;
            StatusCode retVal = init();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;

            try
            {
                streamingClient = new SftpClient(connectionInformation);
                streamingClient.Connect();
                st = streamingClient.OpenRead(transportInformation.SourceFullName);
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException spnex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_FILE_NOT_FOUND, new object[] { connectionInformation.Host, transportInformation.SourceFolderName, transportInformation.SourceFileName }, spnex, Config,ApplicationID);
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_DOWNLOAD_ERROR, new object[] { connectionInformation.Host, transportInformation.SourceFolderName, transportInformation.SourceFileName }, ex, Config,ApplicationID);
            }
            finally
            {

            }

            return retVal;
        }
        public StatusCode Download(Models.FileTransportInfo transportInformation)
        {
            StatusCode retVal = init();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;
            SftpClient client = null;
            try
            {
                using (client = new SftpClient(connectionInformation))
                {
                    client.Connect();

                    using (System.IO.FileStream fs = new System.IO.FileStream(transportInformation.DestinationFullName, System.IO.FileMode.Create))
                    {
                        client.DownloadFile(transportInformation.SourceFullName, fs);

                    }
                    client.Disconnect();
                }
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException spnex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_FILE_NOT_FOUND, new object[] { connectionInformation.Host, transportInformation.SourceFolderName, transportInformation.SourceFileName }, spnex, Config,ApplicationID);
            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.SFTP_DOWNLOAD_ERROR, new object[] { connectionInformation.Host, transportInformation.SourceFolderName, transportInformation.SourceFileName }, ex, Config,ApplicationID);
            }
            finally
            {

            }

            return retVal;
        }


    }
}
