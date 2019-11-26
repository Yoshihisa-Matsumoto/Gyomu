using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_task_info_access_list")]
    public partial class TaskAccessList:BaseDapperFastCrud<TaskAccessList>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long id { get; set; }
        [Key]
        public virtual short application_id { get; set; }
        [Key]
        public virtual short task_info_id { get; set; }
        [Key]
        public virtual string account_name { get; set; }
        public virtual bool can_access { get; set; }
        public virtual bool forbidden { get; set; }
    }
}
