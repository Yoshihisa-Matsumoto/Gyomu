using Dapper.Contrib.Extensions;
using Gyomu.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gyomu.Models
{
    [Table("apps_info_cdtbl")]
    public class ApplicationInfo
    {
        [ExplicitKey]
        public short application_id { get; set; }
        public string description { get; set; }
        public string mail_from_address { get; set; }
        public string mail_from_name { get; set; }
    }
}
