using System;

namespace ScalableTeaching.Models
{
    public class LocalForward
    {
        public int PortNumber { get; set; }
        public Guid MachineID { get; set; }
        public virtual Machine Machine { get; set; }

    }
}
