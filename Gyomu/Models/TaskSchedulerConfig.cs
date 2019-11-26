using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gyomu.Models
{
    [Table("gyomu_task_scheduler_config")]
    public partial class TaskSchedulerConfig:BaseDapperFastCrud<TaskSchedulerConfig>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }
        public virtual short service_id { get; set; }
        public virtual string description { get; set; }
        public virtual short application_id { get; set; }
        public virtual short task_id { get; set; }
        public virtual string monitor_parameter { get; set; }
        public virtual DateTime next_trigger_time { get; set; }
        public virtual string task_parameter { get; set; }
        public virtual bool is_enabled { get; set; }
    }
}
