using backend.Models;
using System;

namespace backend.Controllers
{
    public class CourseDTO
    {
        public Guid CourseID { get; set; }
        public string OwnerUsername { get; set; }
        public string CourseName { get; set; }
        public string ShortCourseName { get; set; }
        public string SDUCourseID { get; set; }

        public static explicit operator CourseDTO(Course c)
        {
            return new CourseDTO
            {
                CourseName = c.CourseName,
                CourseID = c.CourseID,
                OwnerUsername = c.UserUsername,
                SDUCourseID = c.SDUCourseID,
                ShortCourseName = c.ShortCourseName
            };
        }
    }
}