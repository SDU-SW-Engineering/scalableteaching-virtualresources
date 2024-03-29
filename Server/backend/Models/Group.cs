﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class Group
    {
        [Key]
        public Guid GroupID { get; set; }
        public string GroupName { get; set; }
        public int GroupIndex { get; set; }
        public Guid CourseID { get; set; }
        public virtual Course Course { get; set; }

        public virtual List<GroupAssignment> GroupAssignments { get; set; }
        public virtual List<MachineAssignment> MachineAssignments { get; set; }

    }
}
