using System;
using System.Collections.Generic;

namespace ScalableTeaching.DTO
{
    public class FilledGroupDTO
    {
        public string GroupName { get; set; }
        public Guid CourseID { get; set; }
        public List<string> Users { get; set; }
    }
}
