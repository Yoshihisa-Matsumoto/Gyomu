using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_task_instance")]
    public partial class TaskInstance:BaseDapperFastCrud<TaskInstance>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }
        public virtual long task_data_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime entry_date { get; set; }
        public virtual string entry_author { get; set; }
        public virtual string task_status { get; set; }
        public virtual bool is_done { get; set; }
        public virtual long? status_info_id { get; set; }
        public virtual string parameter { get; set; }
        public virtual string comment { get; set; }
    }
}
