using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Common
{
    public class CommonStatusCode:StatusCode
    {
        public static readonly short APP_ID = 0x1;


        public static readonly int DATATABLE_CSV_CONVERT_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x10, "DataTable -> CSV failed", "FileName:{0}\nFields:{1}\nDisplayField:{2}\nSort:{3}");
        public static readonly int EMAIL_SEND_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x11, "Email Sent Failed", "");
        public static readonly int EMAIL_INVALID_SETTING = CODE_GEN(APP_ID, ERROR_DEVEL, 0x12, "SMTP Server setting is not initialized.", "Parameter=SMTPSERVER_PORT");

        public static readonly int ZIP_CONTENTS_ALREADY_EXIST = CODE_GEN(APP_ID, ERROR_DEVEL, 0x13, "Zip Contents {0} in {1} already exists", "");
        public static readonly int ZIP_STORE_FAIL = CODE_GEN(APP_ID, ERROR_DEVEL, 0x14, "Fail to store {0} in {1}", "SourceFile:{2}");
        public static readonly int ZIP_SOURCE_FILE_NOT_EXIST = CODE_GEN(APP_ID, ERROR_DEVEL, 0x15, "Source File doesn't Exist", "{0}\nZip:{1}");
        public static readonly int ZIP_RETRIEVE_FAIL = CODE_GEN(APP_ID, ERROR_DEVEL, 0x16, "Fail to retrieve {0} in {1}", "");
        public static readonly int ZIP_DESTINATION_FILE_NOT_EXIST = CODE_GEN(APP_ID, ERROR_DEVEL, 0x17, "Destination File doesn't exist", "{0}");
        public static readonly int GZIP_DECOMPRESS_FAIL = CODE_GEN(APP_ID, ERROR_DEVEL, 0x18, "GZip Decompression Error", "GZip:{0}");
        public static readonly int BZIP2_DECOMPRESS_FAIL = CODE_GEN(APP_ID, ERROR_DEVEL, 0x19, "Bzip2 Decompression Error", "Bzip2:{0}");

        public static readonly int FTP_DOWNLOAD_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x20, "FTP Download Error", "Server:{0} File:{1} download Error" + System.Environment.NewLine + "State: {2}");
        public static readonly int FTP_UPLOAD_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x21, "FTP Upload Error", "Server:{0} File:{1} upload Error" + System.Environment.NewLine + "State: {2}");
        public static readonly int FTP_INITIALIZE_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x22, "FTP Initialize Error", "Server URL:{0} User:{1} Password:{2}");
        public static readonly int FTP_LISTFILES_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x23, "FTP List File Error", "Server:{0} download Error" + System.Environment.NewLine + "State: {1}");
        public static readonly int FTP_LEGACY_COMMAND_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x24, "FTP Legacy Command Error", "Server:{0} \nCommand:{1}");
        public static readonly int FTP_DELETE_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x25, "FTP Delete Error", "Server:{0} File:{1} delete Error" + System.Environment.NewLine + "State: {2}");

        public static readonly int SFTP_LISTUP_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x26, "Fail to listup directory on SFTP Site", "Server:{0} Directory:{1}");
        public static readonly int SFTP_DOWNLOAD_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x27, "Fail to download file", "Server:{0} Directory:{1} File:{2}");
        public static readonly int SFTP_FILE_NOT_FOUND = CODE_GEN(APP_ID, INFO, 0x28, "File Not Found", "Server:{0} Directory:{1} File:{2}");

        public static readonly int SSH_CONNECT_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x29, "SSH Connect Failed", "Server:{0} UserID:{1}");
        public static readonly int SSH_COMMAND_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x2a, "SSH Command Failed", "Server:{0} UserID:{1}\nCommand:{2}");
        public static readonly int SSH_COMMAND_FAILED = CODE_GEN(APP_ID, ERROR_DEVEL, 0x2b, "SSH Command Failed", "Server:{0} UserID:{1}\nCommand:{2}\nOutMsg:{3}\nError:{4}\nExitCode:{5}");

        public static readonly int SCP_CONNECT_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x2c, "SCP Connect Failed", "Server:{0} UserID:{1}");
        public static readonly int SCP_UPLOAD_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x2d, "SCP Command Failed", "Server:{0} UserID:{1}\nSource Info:{2}\nTarget Path:{3}");
        public static readonly int SCP_DOWNLOAD_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x2e, "SCP Command Failed", "Server:{0} UserID:{1}\nRemote Info:{2}\nTarget Info:{3}");

        public static readonly int FTP_GETTIMESTAMP_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x2f, "FTP Get Modified Time Error", "File:{0} Get Modified Time Error" + System.Environment.NewLine + "State: {1}");
        public static readonly int FTP_GETSIZE_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x30, "FTP Get File Size Error", "File:{0} Get File Size Error" + System.Environment.NewLine + "State: {1}");

        public static readonly int SFTP_UPLOAD_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x31, "SFTP Upload Failed", "Server:{0} Directory:{1} File:{2}");
        public static readonly int SFTP_WRITETIME_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x32, "SFTP Last Write Time Failed", "Server:{0} Directory:{1} File:{2}");

        public static readonly int TASK_GENERATE_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x41, "Task can't be generated", "ApplicationID:{0}\nTask Info ID:{1}\nParameter:{2}");
        public static readonly int TASK_INSTANCE_GENERATE_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x42, "Task Instance can't be generated", "ApplicationID:{0}\nTask Info ID:{1}\nTask Data ID:{2}\nParameter:{3}");
        public static readonly int TASK_ALREADY_GENERATED = CODE_GEN(APP_ID, ERROR_DEVEL, 0x43, "Task is already generated. Can't start twice in the same instance.", "ApplicationID:{0}\nTask Info ID:{1}\nParameter:{2}\nTask Data ID:{3}");
        public static readonly int TASK_EXECUTION_FAILED = CODE_GEN(APP_ID, ERROR_DEVEL, 0x44, "Unknown Exception happens. Developer must handle exception in the target task.", "ApplicationID:{0}\nTask Info ID:{1}\nParameter:{2}\nTask Data ID:{3}");
        public static readonly int TASK_LIBRARY_INTERNAL_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x45, "Task Library Internal Error happens", "Developer must fix the issue,\nApplicationID:{0}\nTask Info ID:{1}\nParameter:{2}\nTask Data ID:{3}");
        public static readonly int TASK_INSTANCE_ALREADY_CHANGED = CODE_GEN(APP_ID, ERROR_BUSINESS, 0x46, "Task already changed", "Task {0} Status already changed since you got before.\nID:{1}");
        public static readonly int TASK_INSTANCE_NOT_EXIST = CODE_GEN(APP_ID, ERROR_DEVEL, 0x47, "Task Instance Not Found", "ApplicationID:{0}\nTask Info ID:{1}\nTask Data ID:{2}\nInstance ID:{3}");
        public static readonly int TASK_STATUS_INCONSISTENT = CODE_GEN(APP_ID, ERROR_DEVEL, 0x48, "There is inconsistent in task status", "ApplicationID:{0}\nTask Info ID:{1}\nTask Data ID:{2}\nInstance ID:{3}\nCurrent Status:{4}\nRequest Status:{5}");
        public static readonly int MAIL_CANNOT_BE_SENT = CODE_GEN(APP_ID, ERROR_BUSINESS, 0x49, "E-mail can't be sent, but process succeeded.Please send by yourself.", "Task {0} ");
        public static readonly int OVERRIDE_METHOD_OTHER_ERROR = CODE_GEN(APP_ID, ERROR_DEVEL, 0x4a, "Overrided Method caused unhandled Exception.", "Developers need to fix this issue.");
        public static readonly int INVALID_USER_ACCESS = CODE_GEN(APP_ID, ERROR_DEVEL, 0x4b, "Invalid User Access Found", "ApplicationID:{0}\nTask Info ID:{1}\nTask Data ID:{2}\nInstance ID:{3}\nTarget Action:{4}\nCurrent User:{5}");
        public static readonly int INVALID_USER_ACCESS_TASKBATCH = CODE_GEN(APP_ID, ERROR_DEVEL, 0x4c, "Invalid User Access Found", "Task Batch ID:{0}\nTask Batch Data ID:{1}\n");
        public static readonly int TASK_CANNOT_BE_INSTANCED = CODE_GEN(APP_ID, ERROR_DEVEL, 0x4d, "Assembly can't be found", "Assembly:{0}\nClass:{1}");

        public static readonly int COMMENT_REQUIRED = CODE_GEN(APP_ID, ERROR_BUSINESS, 0x52, "Comment is mandatory to continue the process", "");


        public CommonStatusCode(int id, Object[] args, Configurator config, short app_id)
            : base(id, args, config,app_id)
        {
        }
        public CommonStatusCode(int id, Object[] args, Exception ex, Configurator config, short app_id)
            : base(id, args, ex, config, app_id)
        {
        }
        public CommonStatusCode(int id, Exception ex, Configurator config, short app_id)
            : base(id, ex, config, app_id)
        {
        }
    }
}
