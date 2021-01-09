using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Port
    {
        public int PortNumber { get; set; }
        public int MachineID { get; set; }
        public virtual Machine Machine { get; set; }

    }
}
