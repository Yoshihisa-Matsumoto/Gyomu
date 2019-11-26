using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Gyomu.Models
{
    [Table("gyomu_task_data_log")]
    public partial class TaskDataLog:BaseDapperFastCrud<TaskDataLog>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }
        public virtual long task_data_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime log_time { get; set; }
        public virtual string log { get; set; }
    }
}
