using FluentFTP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Gyomu.Common.Net
{
    internal class FTP : IDisposable
    {
        private Gyomu.Common.Configurator Config { get; set; }
        private short ApplicationID { get; set; }

        private FtpClient request=null;

        private Models.RemoteConnectionInfo _connectionInformation = null;
        private bool IsConnected
        {
            get
            {
                if (request == null)
                    return false;
                return request.IsConnected;
            }
        }
        private FTP() { }

        public FTP(short applicationId, Gyomu.Common.Configurator config,Models.RemoteConnectionInfo connectionInformation)
        {
            Config = config;
            ApplicationID = applicationId;
            this._connectionInformation = connectionInformation;
        }
        private StatusCode init()
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            if (IsConnected)
                return retVal;
            try
            {
                if (string.IsNullOrEmpty(_connectionInformation.ProxyHost))
                {
                    request = new FtpClient(_connectionInformation.ServerURL)
                    {
                        Port = _connectionInformation.Port
                    };
                }
                else
                {
                    NetworkCredential proxyCredential = new NetworkCredential(_connectionInformation.ProxyUserID, _connectionInformation.ProxyPassword);
                    FluentFTP.Proxy.ProxyInfo proxyInfo = new FluentFTP.Proxy.ProxyInfo() { Host = _connectionInformation.ProxyHost, Port = _connectionInformation.ProxyPort, Credentials = proxyCredential };

                    request = new FluentFTP.Proxy.FtpClientHttp11Proxy(proxyInfo)
                    {
                        Host = _connectionInformation.ProxyHost,
                        Port = _connectionInformation.Port
                    };
                }

                request.SocketKeepAlive = false;
                request.DataConnectionType = _connectionInformation.IsPassive ? FtpDataConnectionType.AutoPassive : FtpDataConnectionType.AutoActive;
                request.EncryptionMode = _connectionInformation.SslEnabled ? (_connectionInformation.SslImplicit ? FtpEncryptionMode.Implicit : FtpEncryptionMode.Explicit) : FtpEncryptionMode.None;
                request.SslProtocols = _connectionInformation.SslEnabled ? System.Security.Authentication.SslProtocols.Tls : System.Security.Authentication.SslProtocols.None;
                if (_connectionInformation.SslEnabled)
                    request.ValidateCertificate += request_ValidateCertificate;

                if (string.IsNullOrEmpty(_connectionInformation.UserID) == false)
                {
                    string password = "";
                    if (string.IsNullOrEmpty(_connectionInformation.Password) == false)
                    {
                        password = _connectionInformation.Password;
                    }
                    request.Credentials = new NetworkCredential(_connectionInformation.UserID, password);
                }
                
                request.Connect();
            }
            catch (Exception ex)
            {
                object[] args = new object[3];
                args[0] = _connectionInformation.ServerURL;
                string user_id;
                string password;
                if (string.IsNullOrEmpty(_connectionInformation.UserID) == false)
                {
                    user_id = _connectionInformation.UserID;
                    password = "";
                    if (string.IsNullOrEmpty(_connectionInformation.Password) == false)
                    {
                        password = _connectionInformation.Password;
                    }

                }
                else
                {
                    user_id = "";
                    password = "";
                }
                args[1] = user_id;
                args[2] = password;
                return new Common.CommonStatusCode(CommonStatusCode.FTP_INITIALIZE_ERROR, args, ex, Config, ApplicationID);
            }
            return retVal;
        }

        private void request_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public StatusCode Download(Models.FileTransportInfo transportInformation,bool isBinary)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            try
            {
                retVal = init();
                if (retVal.IsSucceeded == false)
                    return retVal;
                request.DownloadDataType = isBinary ? FtpDataType.Binary : FtpDataType.ASCII;
                bool result = request.DownloadFile(transportInformation.DestinationFullName, transportInformation.SourceFullName, FtpLocalExists.Overwrite, FtpVerify.Retry);
                if (result == false)
                {
                    return new Common.CommonStatusCode(Common.CommonStatusCode.FTP_DOWNLOAD_ERROR, new object[] { transportInformation.SourceFullName, transportInformation.DestinationFullName, "Not sure" }, Config, ApplicationID);
                }

            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.FTP_DOWNLOAD_ERROR, new object[] { transportInformation.SourceFullName, transportInformation.DestinationFullName, "Not sure" }, ex, Config, ApplicationID);
            }
            return StatusCode.SUCCEED_STATUS;
        }
        public StatusCode Upload(Models.FileTransportInfo transportInformation, bool isBinary)
        {
            StatusCode retVal = StatusCode.SUCCEED_STATUS;
            try
            {
                retVal = init();
                if (retVal.IsSucceeded == false)
                    return retVal;
                request.UploadDataType = isBinary ? FtpDataType.Binary : FtpDataType.ASCII;
                bool result = request.UploadFile(transportInformation.SourceFullName, transportInformation.DestinationFullName, FtpExists.Overwrite);
                if (result == false)
                {
                    return new Common.CommonStatusCode(Common.CommonStatusCode.FTP_DOWNLOAD_ERROR, new object[] { transportInformation.SourceFullName, transportInformation.DestinationFullName, "Not sure" }, Config, ApplicationID);
                }

            }
            catch (Exception ex)
            {
                return new Common.CommonStatusCode(Common.CommonStatusCode.FTP_DOWNLOAD_ERROR, new object[] { transportInformation.SourceFullName, transportInformation.DestinationFullName, "Not sure" }, ex, Config, ApplicationID);
            }
            return StatusCode.SUCCEED_STATUS;
        }
        public StatusCode GetFileInfo(Models.FileTransportInfo transportInformation, out long lSize, out DateTime updateTime)
        {
            lSize = -1;
            updateTime = DateTime.Today;
            StatusCode retVal = init();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;

            try
            {
                
                FtpListItem item = request.GetObjectInfo(transportInformation.SourceFullName);
                lSize = item.Size;
                updateTime = item.Modified;
            }
            catch (Exception ex)
            {
                object[] args = new object[2];
                args[0] = transportInformation.SourceFullName;
                args[1] = "**";
                return new Common.CommonStatusCode(Common.CommonStatusCode.FTP_GETSIZE_ERROR, args, ex, Config, ApplicationID);
            }
            return retVal;
        }
        public StatusCode ListFiles(Models.FileTransportInfo transportInformation, out List<string> lstFile)
        {
            StatusCode retVal = init();
            lstFile = new List<string>();
            if (retVal != StatusCode.SUCCEED_STATUS)
                return retVal;

            try
            {
                foreach (FtpListItem item in request.GetListing(transportInformation.SourceFolderName))
                {
                    if (item.Type == FtpFileSystemObjectType.File)
                    {
                        if (string.IsNullOrEmpty(transportInformation.SourceFileName))
                            lstFile.Add(item.Name);
                        else
                        {
                            if (Regex.IsMatch(item.Name, transportInformation.SourceFileName))
                                lstFile.Add(item.Name);
                        }
                    }
                }

            }
            catch (Exception){}
            return retVal;
        }

        public void Dispose()
        {
            if (request != null && request.IsConnected)
                request.Disconnect();

        }
    }
}
