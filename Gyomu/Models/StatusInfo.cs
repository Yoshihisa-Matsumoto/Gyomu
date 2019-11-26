using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_status_info")]
    public partial class StatusInfo : BaseDapperFastCrud<StatusInfo>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }
        public virtual short application_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime entry_date { get; set; }
        public virtual string entry_author { get; set; }
        public virtual short status_type { get; set; }
        public virtual short error_id { get; set; }
        public virtual int instance_id { get; set; }
        public virtual string hostname { get; set; }
        public virtual string summary { get; set; }
        public virtual string description { get; set; }
        public virtual string developer_info { get; set; }
    }
}
