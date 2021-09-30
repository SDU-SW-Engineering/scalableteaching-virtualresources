using System;
using System.Collections.Generic;

namespace ScalableTeaching.DTO
{
    public class CreateMachineGroupBased
    {
        public List<CreateMachineGroupBasedSingle> Machines { get; set; }
    }
    public class CreateMachineGroupBasedSingle
    {
        public string Hostname { get; set; }
        public Guid Group { get; set; }
        public List<string> Apt { get; set; }
        public List<string> Ppa { get; set; }
        public List<int> Ports { get; set; }
        public List<string> LinuxGroups { get; set; }
        public Guid CourseID { get; set; }
        public int? Memory { get; set; }
        public int? VCPU { get; set; }
        public int? Storage { get; set; }
    }
}
