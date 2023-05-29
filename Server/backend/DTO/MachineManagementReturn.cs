using System;
using System.Collections.Generic;

namespace ScalableTeaching.DTO
{
    public class MachineManagementReturn
    {
        public Guid MachineID { get; set; }
        public string Hostname { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public CourseDTO Course { get; set; }
        public List<string> Users { get; set; }
        public List<int> Ports { get; set; }

        public int Size { get; set; }
    }
}
