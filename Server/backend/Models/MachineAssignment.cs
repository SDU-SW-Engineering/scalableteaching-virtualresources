using System;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class MachineAssignment
    {
        public Guid MachineID { get; set; }
        [Required]
        public string UserUsername { get; set; }

#nullable enable
        public string? OneTimePassword { get; set; }
#nullable restore

        public Guid? GroupID { get; set; }

        public virtual User User { get; set; }
        public virtual Machine Machine { get; set; }
        public virtual Group Group { get; set; }

    }
}
