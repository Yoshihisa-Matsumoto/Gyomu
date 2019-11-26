using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Gyomu.Models
{
    [Table("gyomu_task_data_status")]
    public partial class TaskDataStatus:BaseDapperFastCrud<TaskDataStatus>
    {
        [Key]
        public virtual long task_data_id { get; set; }
        public virtual string task_status { get; set; }
        public virtual DateTime latest_update_date { get; set; }
        public virtual long latest_task_instance_id { get; set; }
    }
}
