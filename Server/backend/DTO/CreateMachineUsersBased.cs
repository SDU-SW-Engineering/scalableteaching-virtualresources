using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.DTO
{
    public class CreateMachineUsersBased
    {
        public List<CreateMachineUsersBasedSingle> Machines { get; set; }
    }
    public class CreateMachineUsersBasedSingle
    {
        public string Hostname { get; set; }
        public List<string> Users { get; set; }
        public List<string> Apt { get; set; }
        public List<string> Ppa { get; set; }
        public List<int> Ports { get; set; }
        public List<string> LinuxGroups { get; set; }
        public Guid CourseID { get; set; }
    }
}
