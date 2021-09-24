using System;
using System.ComponentModel.DataAnnotations;

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
