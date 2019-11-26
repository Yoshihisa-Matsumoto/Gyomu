
using Gyomu.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_apps_info_cdtbl")]
    public partial class ApplicationInfo:BaseDapperFastCrud<ApplicationInfo>
    {
        [Key]
        public virtual short application_id { get; set; }
        public virtual string description { get; set; }
        public virtual string mail_from_address { get; set; }
        public virtual string mail_from_name { get; set; }
    }
}
