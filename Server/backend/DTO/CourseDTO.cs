using backend.Models;
using System;

namespace backend.Controllers
{
    public class CourseDTO
    {
        public Guid CouseID { get; set; }
        public string OwnerUsername { get; set; }
        public string CourseName { get; set; }
        public string ShortCourseName { get; set; }
        public string SDUCourseID { get; set; }

        public static explicit operator CourseDTO(Course c)
        {
            return new CourseDTO
            {
                CourseName = c.CourseName,
                CouseID = c.CourseID,
                OwnerUsername = c.UserUsername,
                SDUCourseID = c.SDUCourseID,
                ShortCourseName = c.ShortCourseName
            };
        }
    }
}