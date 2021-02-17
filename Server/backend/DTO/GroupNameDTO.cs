using System;

namespace backend.Controllers
{
    public class GroupNameDTO
    {
        public Guid GroupID { get; set; }
        public string GroupName { get; set; }
        public Guid CourseID { get; set; }
    }
}