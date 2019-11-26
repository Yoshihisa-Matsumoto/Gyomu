using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gyomu.Models
{
    [Table("gyomu_status_handler")]
    public partial class StatusHandler:BaseDapperFastCrud<StatusHandler>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int id { get; set; }
        public virtual short application_id { get; set; }
        public virtual string region { get; set; }
        public virtual short? status_type { get; set; }
        public virtual string recipient_address { get; set; }
        public virtual string recipient_type { get; set; }
    }
}
