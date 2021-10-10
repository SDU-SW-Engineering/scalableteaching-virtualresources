using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScalableTeaching.Models
{
    public class Course
    {
        [Key]
        public Guid CourseID { get; set; }

        [Required]
        [ForeignKey("Username")]
        [Comment("The user responsible for the course i.e. the user that can make machines associated with the course")]
        public string UserUsername { get; set; }

        [Required]
        [MinLength(3)]
        public string CourseName { get; set; }

        [Required]
        [Comment("Should be between 3 and 6 characters")]
        [MinLength(3)]
        [MaxLength(6)]
        public string ShortCourseName { get; set; }

        [Required]
        public string SDUCourseID { get; set; }
        public virtual User User { get; set; }

        public static (bool, string) Validate(string UserUsername, string CourseName, string ShortCourseName, string SDUCourseID)
        {
            if (UserUsername == null) return (false, "UserUsername null");
            if (CourseName == null) return (false, "CourseName null");
            if (ShortCourseName == null) return (false, "ShortCourseName null");
            if (SDUCourseID == null) return (false, "SDUCourseID null");
            if (UserUsername.Length < 3) return (false, "Invalid Username Length");
            if (CourseName.Length < 3) return (false, "Empty Coursename");
            if (ShortCourseName.Length < 3) return (false, "ShortCourseName less that 3 char long");
            if (ShortCourseName.Length > 6) return (false, "ShortCourseName more that 6 char long");
            if (SDUCourseID.Length < 4) return (false, "Invalid SDU Course ID");
            return (true, "No error detected");
        }
    }
}
