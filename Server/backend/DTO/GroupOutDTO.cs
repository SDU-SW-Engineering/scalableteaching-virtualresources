using ScalableTeaching.Models;
using System;

namespace ScalableTeaching.DTO
{
    public class GroupOutDTO
    {
        public string GroupName { get; set; }
        public Guid CourseID { get; set; }
        public int GroupIndex { get; set; }

        public Guid GroupID { get; set; }

        public static explicit operator GroupOutDTO(Group group)
        {
            return new GroupOutDTO { GroupName = group.GroupName, GroupIndex = group.GroupIndex, CourseID = group.CourseID, GroupID = group.GroupID };
        }
    }
}
