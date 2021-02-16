using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Group
    {
        public Guid GroupID { get; set; }
        public string GroupName { get; set; }
        public string CourseID { get; set; }
        public virtual Course Course { get; set; }

    }
}
