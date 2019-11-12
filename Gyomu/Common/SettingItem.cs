using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gyomu.Common
{
    public class SettingItem
    {
        #region Environment Variable
        [Description("Normally either PROD UAT DEV")]
        internal const string GYOMU_COMMON_MODE = "GYOMU_COMMON_MODE";
        [Description("DB Connection String")]
        internal const string GYOMU_COMMON_MAINDB_CONNECTION = "GYOMU_COMMON_MAINDB_CONNECTION";
        [Description("Support DB would be MSSQL or POSTGRESQL")]
        internal const string GYOMU_COMMON_MAINDB_TYPE = "GYOMU_COMMON_MAINDB_TYPE";
        #endregion

        #region ParamMaster
        [Description("SMTP Server + Port")]
        internal const string SMTPSERVER_PORT = "SMTPSERVER_PORT";
        [Description("SMTP Server for external recipient if necessary + Port")]
        internal const string SMTPEXTERNALSERVER_PORT = "SMTPEXTERNALSERVER_PORT";
        [Description("Domain list to recognize as internal. Must be JSON serialized")]
        internal const string SMTP_TARGETINTERNALDOMAINS = "SMTP_TARGETINTERNALDOMAINS";
        #endregion
        public enum DBType
        {
            MSSQL,
            POSTGRESQL,
            Other
        }
        //internal static string MSSQL {get{return "MSSQL";} }
        //internal static string POSTGRESQL {get{return "POSTGRESQL";}}
    }
}
