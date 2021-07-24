using ScalableTeaching.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.DTO
{
    public class CreateMachineDTO
    {
        public string MachineName { get; set; }
        public Guid ClassID { get; set; }
        public ReplicationDirectives ReplicationDirective { get; set; }


    }
    
}
