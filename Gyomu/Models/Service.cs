using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gyomu.Models
{
    [Table("gyomu_service_cdtbl")]
    public partial class Service:BaseDapperFastCrud<Service>
    {
        [Key]
        public virtual short id { get; set; }
        public virtual string description { get; set; }
        public virtual short service_type_id { get; set; }
        public virtual string parameter { get; set; }
    }
}
