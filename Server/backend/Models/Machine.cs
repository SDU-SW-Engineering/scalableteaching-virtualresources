using System;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class Machine
    {
        [Key]
        public Guid MachineID { get; set; }
        [Required]
        public string Name { get; set; }
        public string HostName { get; set; }
        [Required]
        public string UserUsername  { get; set; }
        [Required]
        public Guid CourseID { get; set; }

        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
    }
}
