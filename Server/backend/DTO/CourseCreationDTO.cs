using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.DTO
{
    public class CourseCreationDTO
    {
        public string OwnerUsername { get; set; }
        public string CourseName { get; set; }
        public string ShortCourseName { get; set; }
        public string SDUCourseID { get; set; }
    }
}
