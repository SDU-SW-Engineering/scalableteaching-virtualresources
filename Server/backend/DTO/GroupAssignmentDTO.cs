using ScalableTeaching.Models;
using System;

namespace ScalableTeaching.DTO
{
    public class GroupAssignmentDTO
    {
        public Guid GroupID { get; set; }
        public string UserUsername { get; set; }

        public static implicit operator GroupAssignmentDTO(GroupAssignment groupAssignment)
        {
            return new GroupAssignmentDTO() { GroupID = groupAssignment.GroupID, UserUsername = groupAssignment.UserUsername };
        }
    }
}
