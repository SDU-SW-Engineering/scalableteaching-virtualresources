using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO
{
    public class GroupOutDTO
    {
        public string GroupName { get; set; }
        public Guid CourseID { get; set; }

        public Guid GroupID { get; set; }

        public static explicit operator GroupOutDTO(Group group)
        {
            return new GroupOutDTO { GroupName = group.GroupName, CourseID = group.CourseID, GroupID = group.GroupID };
        }
    }
}
