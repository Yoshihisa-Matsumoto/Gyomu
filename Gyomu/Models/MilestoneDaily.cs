using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_milestone_daily")]
    public partial class MilestoneDaily:BaseDapperFastCrud<MilestoneDaily>
    {
        [Key]
        public virtual string target_date { get; set; }
        [Key]
        public virtual string milestone_id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime update_time { get; set; }
    }
}
