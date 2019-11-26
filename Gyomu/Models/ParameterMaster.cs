using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_param_master")]
    public partial class ParameterMaster:BaseDapperFastCrud<ParameterMaster>
    {
        [Key]
        public virtual string item_key { get; set; }
        public virtual string item_value { get; set; }
        [Key]
        public virtual string item_fromdate { get; set; }
    }
}
