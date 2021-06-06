using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class FilledGroupDTO
    {
        public string GroupName { get; set; }
        public Guid CourseID { get; set; }
        public List<string> Users{ get; set; }
    }
}
