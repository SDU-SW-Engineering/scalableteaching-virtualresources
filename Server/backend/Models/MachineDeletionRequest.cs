using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalableTeaching.Models
{
    public class MachineDeletionRequest
    {
        [Key]
        public Guid MachineID { get; set; }
        public DateTime DeletionDate { get; set; }
        public string UserUsername { get; set; }

        public virtual Machine Machine { get; set; }
        public virtual User User { get; set; }
    }
}
