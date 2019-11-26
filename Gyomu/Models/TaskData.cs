using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_task_data")]
    public partial class TaskData:BaseDapperFastCrud<TaskData>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }
        public virtual short application_id { get; set; }
        public virtual short task_info_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime entry_date { get; set; }
        public virtual string entry_author { get; set; }
        public virtual long? parent_task_data_id { get; set; }
        public virtual string parameter { get; set; }
    }
}
