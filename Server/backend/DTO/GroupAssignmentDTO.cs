using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO
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
