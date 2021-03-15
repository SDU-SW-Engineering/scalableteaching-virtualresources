using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class MachineAssignment
    {
        public Guid MachineID { get; set; }
        [Required]
        public string UserUsername { get; set; }
        
        public Guid? GroupID { get; set; }
        
        public virtual User User { get; set; }
        public virtual Machine Machine { get; set; }
        public virtual Group Group { get; set; }
        
    }
}
