using System;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class MachineAssignment
    {
        [Key]
        public Guid MachineAssignmentID { get; set; }
        public Guid MachineID { get; set; }
#nullable enable
        public string? UserUsername { get; set; }
        public Guid? GroupID { get; set; }
        public string? OneTimePassword { get; set; }
#nullable restore

        public virtual Machine Machine { get; set; }

    }
}
