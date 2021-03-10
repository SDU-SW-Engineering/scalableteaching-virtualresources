using System;

namespace backend.Controllers
{
    public class CourseDTO
    {
        public Guid CouseID { get; set; }
        public string OwnerUsername { get; set; }
        public string CourseName { get; set; }
        public string SDUCourseID { get; set; }
    }
}