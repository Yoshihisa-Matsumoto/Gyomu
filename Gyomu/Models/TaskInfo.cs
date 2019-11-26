using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_task_info_cdtbl")]
    public partial class TaskInfo:BaseDapperFastCrud<TaskInfo>
    {
        [Key]
        public virtual short application_id { get; set; }
        [Key]
        public virtual short task_id { get; set; }
        public virtual string description { get; set; }
        public virtual string assembly_name { get; set; }
        public virtual string class_name { get; set; }
        public virtual bool restartable { get; set; }
    }
}
