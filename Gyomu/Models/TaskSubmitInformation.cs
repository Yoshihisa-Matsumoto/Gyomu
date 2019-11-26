using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_task_instance_submit_information")]
    public partial class TaskSubmitInformation:BaseDapperFastCrud<TaskSubmitInformation>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }

        public virtual long task_instance_id { get; set; }

        public virtual string submit_to { get; set; }
    }
}
