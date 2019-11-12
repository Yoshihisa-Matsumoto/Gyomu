using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace Gyomu
{
    public class StatusCode
    {
        protected int _error_id;
        private readonly Object[] _arguments;
        private readonly System.Diagnostics.StackFrame _frame;
        private Exception _ex;
        private Common.Configurator _config;

        private int _instance_id;
        private readonly string _region;

        //When you need to override, set it in the child class. This is only for Common Library
        protected short _application_id = 0;

        public const short INFO = 0;
        public const short WARNING = 1;
        public const short ERROR_BUSINESS = 2;
        public const short ERROR_BUSINESS2 = 3;
        public const short ERROR_BUSINESS3 = 4;
        public const short ERROR_DEVEL = 8;

        public static readonly string ALL_REGION = "#";

        public static StatusCode SUCCEED_STATUS;
        public bool IsSucceeded { get { return this == SUCCEED_STATUS; } }
        public bool IsNotFailed { get { return this.StatusType < ERROR_BUSINESS; } }

        public static int SUCCEED = 0;
        //public static StatusCode OTHER_ERROR_STATUS;
        public static int OTHER_ERROR = CODE_GEN(0, ERROR_DEVEL, 1);
        public static int IO_ERROR = CODE_GEN(0, ERROR_DEVEL, 2);
        private static readonly int DEBUG_COMMENT = CODE_GEN(0, ERROR_DEVEL, 4);

        private static readonly int INVALID_ARGUMENT_ERROR = CODE_GEN(0, ERROR_DEVEL, 5);

        private static System.Collections.Hashtable tbl = null;
        protected static System.Collections.Hashtable STATUS_TABLE { get { return tbl; } }

        public int ErrorID { get { return _error_id; } }
        public Object[] Arguments { get { return _arguments; } }

        private long _statuc_info_id = Int32.MinValue;
        private AutoResetEvent _db_registered = new AutoResetEvent(false);
        public long StatusID
        {
            get
            {
                if (_statuc_info_id == Int32.MinValue)
                    _db_registered.WaitOne();
                return _statuc_info_id;
            }
        }

        protected StatusCode() { _error_id = 0; }
        
        public StatusCode(int id, Object[] arguments, Common.Configurator config)
        {
            _error_id = id;
            _arguments = arguments;
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            _frame = trace.GetFrame(1);
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _config = config;
            _application_id = _config.ApplicationID;
            doRegister();
        }
        public StatusCode(int id, Common.Configurator config)
        {
            _error_id = id;
            _arguments = null;
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            _frame = trace.GetFrame(1);
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _config = config;
            _application_id = _config.ApplicationID;
            doRegister();
        }
        public StatusCode(int id, Exception ex, Common.Configurator config)
        {
            _error_id = id;
            _arguments = null;
            //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            //_frame = trace.GetFrame(1);
            _ex = ex;
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _config = config;
            _application_id = _config.ApplicationID;
            doRegister();
        }
        
        public StatusCode(int id, Object[] arguments, Exception ex, Common.Configurator config)
        {
            _error_id = id;
            _arguments = arguments;
            _ex = ex;
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _config = config;
            _application_id = _config.ApplicationID;
            doRegister();
        }
        //For Inherited Class Constructor
        protected StatusCode(int id, Object[] arguments, Common.Configurator config, short app_id)
        {
            _error_id = id;
            _arguments = arguments;
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            _frame = trace.GetFrame(1);
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _application_id = app_id;
            _config = config;
            doRegister();
        }
        protected StatusCode(int id, Common.Configurator config, short app_id)
        {
            _error_id = id;
            _arguments = null;
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            _frame = trace.GetFrame(1);
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _application_id = app_id;
            _config = config;
            doRegister();
        }
        protected StatusCode(int id, Exception ex, Common.Configurator config, short app_id)
        {
            _error_id = id;
            _arguments = null;
            //System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);
            //_frame = trace.GetFrame(1);
            _ex = ex;
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _application_id = app_id;
            _config = config;
            doRegister();
        }
        protected StatusCode(int id, Object[] arguments, Exception ex, Common.Configurator config, short app_id)
        {
            _error_id = id;
            _arguments = arguments;
            _ex = ex;
            _instance_id = config.UniqueInstanceIDPerMachine;
            _region = config.Region;
            _config = config;
            _application_id = app_id;
            doRegister();
        }
        public static StatusCode GetStatusCode(long status_info_id)
        {
            if (status_info_id == 0)
                return StatusCode.SUCCEED_STATUS;

            Models.StatusInfo statusInfo = Common.GyomuDataAccess.SelectStatusInfo(status_info_id);
            if (statusInfo == null)
                return null;
            StatusCode retVal = new StatusCode
            {
                _statuc_info_id = status_info_id
            };


            if (string.IsNullOrEmpty(statusInfo.summary))
                retVal._internalSummary = "";
            else
                retVal._internalSummary = statusInfo.summary;
            if (string.IsNullOrEmpty(statusInfo.description))
                retVal._internalDescription = "";
            else
                retVal._internalDescription = statusInfo.description;
            if (string.IsNullOrEmpty(statusInfo.developer_info))
                retVal._internalStackTrace = "";
            else
                retVal._internalStackTrace = statusInfo.developer_info;
            retVal._error_id = statusInfo.error_id;
            return retVal;
        }
        static StatusCode()
        {
            //Console.SetOut(writer);

            tbl = new System.Collections.Hashtable();
            Add(SUCCEED, "Succeeded", "");
            SUCCEED_STATUS = new StatusCode();
            Add(OTHER_ERROR, "Other Error", "Other Error happened");
            Add(IO_ERROR, "IO Error", "IO Error happened.");
            Add(DEBUG_COMMENT, "Debug {0}", "Debug");
            Add(INVALID_ARGUMENT_ERROR, "Invalid Argument", "{0}");

        }
        public override string ToString()
        {
            StatusInfo info = (StatusInfo)tbl[this._error_id];

            if (_internalSummary != null)
            {
                return _internalSummary + System.Environment.NewLine + _internalDescription + System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine + _internalStackTrace;
            }
            string strMsg = info.Summary + System.Environment.NewLine + info.Description;

            string caller = "";
            if (this._ex != null)
            {
                Exception trueEx = _ex;
                while (trueEx.InnerException != null)
                    trueEx = _ex.InnerException;
                caller = System.Environment.NewLine + trueEx.Message + System.Environment.NewLine + trueEx.StackTrace + System.Environment.NewLine + getExceptionInformation(trueEx);
            }
            else
            {
                if (this._frame != null)
                {
                    caller = System.Environment.NewLine + this._frame.GetFileName() + "#" + this._frame.GetMethod().Name + "(" + this._frame.GetFileLineNumber().ToString() + ")";
                }
            }

            if (this._arguments == null)
                return strMsg + caller;
            return String.Format(strMsg, this._arguments) + caller;
        }

        private string getExceptionInformation(Exception ex)
        {
            try
            {
                StringBuilder strBuf = new StringBuilder();
                if (ex is System.Reflection.ReflectionTypeLoadException rte)
                {
                    for (int i = 0; i < rte.Types.Length; i++)
                    {
                        Type t = rte.Types[i];
                        Exception ie = rte.LoaderExceptions[i];

                        strBuf.AppendLine("Type:" + t == null ? "" : t.FullName);
                        strBuf.AppendLine("Exception:" + ie == null ? "" : ie.ToString());

                    }
                }
                return strBuf.ToString();
            }
            catch (Exception )
            {
                return "";
            }
        }

        private string _internalSummary = null;
        private string _internalDescription = null;
        private string _internalStackTrace = null;
        private string getTitle()
        {
            string strMsg = null;
            lock (tbl)
            {
                StatusInfo info = (StatusInfo)tbl[this._error_id];
                strMsg = info.Summary;
            }

            if (_arguments == null)
                return strMsg;
            return String.Format(strMsg, Arguments);
        }
        public string Summary
        {
            get
            {
                if (_internalSummary != null)
                {
                    return _internalSummary + System.Environment.NewLine + _internalDescription;
                }
                string strMsg = null;
                lock (tbl)
                {
                    StatusInfo info = (StatusInfo)tbl[this._error_id];
                    strMsg = info.Summary + System.Environment.NewLine + info.Description;
                }

                if (_arguments == null)
                    return strMsg;
                return String.Format(strMsg, Arguments);
            }
        }
        public string Message
        {
            get
            {
                if (_internalDescription != null)
                {
                    return _internalDescription;
                }

                string strMsg = null;
                lock (tbl)
                {
                    StatusInfo info = (StatusInfo)tbl[this._error_id];
                    strMsg = info.Description;
                }

                if (_arguments == null)
                    return strMsg;
                return String.Format(strMsg, Arguments);
            }
        }
        public string GetCallerInfo()
        {
            if (_internalStackTrace != null)
            {
                return _internalStackTrace;
            }

            string caller = "";
            if (this._ex != null)
            {
                caller = this._ex.Message + System.Environment.NewLine + this._ex.StackTrace;
                if (_ex.InnerException != null)
                {
                    caller = caller + System.Environment.NewLine + "======================" + System.Environment.NewLine + "Inner Exception" + System.Environment.NewLine + _ex.InnerException.Message + System.Environment.NewLine + _ex.InnerException.StackTrace;
                }
            }
            else
            {
                if (this._frame != null)
                {
                    caller = this._frame.GetFileName() + "#" + this._frame.GetMethod().Name + "(" + this._frame.GetFileLineNumber().ToString() + ")";
                }
            }
            return caller;
        }
        protected static void Add(int id, string summary, string description)
        {
            tbl.Add(id, new StatusInfo(summary, description));
        }

        protected static int CODE_GEN(short app, short type, short code)
        {
            if (app > 0xFFF || type > 0xF )
                throw new Exception("Invalid Code");
            return (int)(app << 20) + (int)(type << 16) + (code);
        }
        protected static int CODE_GEN(short app, short type, short code, string summary, string description)
        {
            if (app > 0xFFF || type > 0xF )
                throw new Exception("Invalid Code");
            int error_code = (int)(app << 20) + (int)(type << 16) + (code);

            Add(error_code, summary, description);

            return error_code;
        }
        public short AppCode
        {
            get
            {
                return (short)((_error_id >> 20) & 0xFFF);
            }
        }
        public short StatusType
        {
            get
            {
                return Convert.ToInt16((_error_id & 0xFFFFF) >> 16);
            }
        }
        private short getCode()
        {
            return Convert.ToInt16(_error_id & 0xFFFF);
        }
        private void doRegister()
        {

            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(this._doRegister));
            th.Start();
        }
        
        private void _doRegister()
        {
            try
            {
                Models.ApplicationInfo applicationInformation = null;

                short application_id = _application_id == 0 ? AppCode : _application_id;
                string information = Message;
                string developer_information = GetCallerInfo();
                short status_type = StatusType;
                short status_code = getCode();
                string title = getTitle();
                List<Models.StatusHandler> lstMailHandler = null;

                try
                {
                    using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew))
                    {

                        applicationInformation = Common.GyomuDataAccess.SelectApplicationInfo(application_id);
                        Models.StatusInfo statusInfo = new Models.StatusInfo()
                        {
                            application_id = application_id,
                            entry_author = Common.User.CurrentUser.UserID,
                            status_type = status_type,
                            error_id = status_code,
                            instance_id = _instance_id,
                            hostname = System.Environment.MachineName,
                            summary = title,
                            description = information,
                            developer_info = developer_information
                        };
                        _statuc_info_id = Common.GyomuDataAccess.InsertStatusInfo(statusInfo);
                        lstMailHandler = Common.GyomuDataAccess.SelectStatusHandlers(application_id);    
                        
                        scope.Complete();
                        
                    }
                    if (applicationInformation != null && lstMailHandler != null && lstMailHandler.Count > 0 && (string.IsNullOrEmpty(Common.Net.Email.SmtpServerPortSplitWithColon)==false))
                    {
                        Common.Net.Email email = new Common.Net.Email(application_id, _config);
                        string[] to = lstMailHandler.Where(s => s.recipient_type.Equals("TO")).Select(h => h.recipient_address).ToArray();
                        string[] cc = lstMailHandler.Where(s => s.recipient_type.Equals("CC")).Select(h => h.recipient_address).ToArray();
                        StringBuilder strBuf = new StringBuilder("<HTML><BODY><pre>");
                        strBuf.Append(getMailBody());
                        strBuf.Append("</pre></BODY></HTML>");

                        string subject = applicationInformation.description + " " + getTitle();
                        subject = subject.Replace('\r', ' ').Replace('\n', ' ');

                        StatusCode retVal = email.Send(applicationInformation.mail_from_address, applicationInformation.mail_from_name,
                          to, cc, subject, strBuf.ToString(), null);

                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    _db_registered.Set();
                }

                
                

            }
            catch (Exception) { }
        }
        
        private string getMailBody()
        {
            StringBuilder strBuf = new StringBuilder();
            strBuf.Append("User:" + Common.User.CurrentUser.UserID + "\tMachine:" + System.Environment.MachineName + "\tInstance:" + _instance_id.ToString() + "\n");
            if (Convert.ToInt32(StatusType) > ERROR_BUSINESS)
            {
                //For Developer
                strBuf.Append(Message);
                strBuf.Append("\n");
                strBuf.Append(GetCallerInfo());
            }
            else
            {
                //For General User
                strBuf.Append(Message);
            }
            return strBuf.ToString();//strBuf.Replace("\n", "\n<BR/>").Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").ToString();
        }
        public static StatusCode Debug(string argument, Common.Configurator config)
        {
            object[] arguments = new object[1];
            arguments[0] = argument;
            return new StatusCode(DEBUG_COMMENT, arguments, config);
        }
        
        public static StatusCode InvalidArgumentStatus(string summary, Common.Configurator config, short app_id)
        {
            object[] arguments = new object[1];
            arguments[0] = summary;
            return new StatusCode(INVALID_ARGUMENT_ERROR, arguments, config, app_id);
        }
    }
    class StatusInfo
    {
        public string Summary { get; private set; }
        public string Description { get; private set; } 
        internal StatusInfo(string summary, string description)
        {
            Summary = summary;
            Description = description;
        }
    }
}
