using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class MachineCredentail
    {
        public Guid MachineID { get; set; }
        public Guid UserID { get; set; }
        public virtual User User { get; set; }
        public virtual Machine Machine { get; set; }
    }
}
