using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Course
    {
        [Key]
        public Guid CouseID { get; set; }
        [Required]
        [Comment("The user responsible for the course i.e. the user that can make machines associated with the course")]
        public Guid UserID { get; set; }
        [Required]
        public string CourseName { get; set; }
        [Required]
        public string SDUCourseID { get; set; }
        public virtual User User { get; set; }
    }
}
