using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Machine
    {
        [Key]
        public Guid MachineID { get; set; }
        [Required]
        public string Name { get; set; }
        public string HostName { get; set; }
        [Required]
        public Guid UserID  { get; set; }
        [Required]
        public Guid CourseID { get; set; }

        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
    }
}
