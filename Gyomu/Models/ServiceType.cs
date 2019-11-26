using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gyomu.Models
{
    [Table("gyomu_service_type_cdtbl")]
    public partial class ServiceType:BaseDapperFastCrud<ServiceType>
    {
        [Key]
        public virtual short id { get; set; }
        public virtual string description { get; set; }
        public virtual string assembly_name { get; set; }
        public virtual string class_name { get; set; }
    }
}
