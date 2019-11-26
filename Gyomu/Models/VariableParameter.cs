using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_variable_parameter")]
    public partial class VariableParameter:BaseDapperFastCrud<VariableParameter>
    {
        [Key]
        public virtual string variable_key { get; set; }
        public virtual string description { get; set; }
    }
}
