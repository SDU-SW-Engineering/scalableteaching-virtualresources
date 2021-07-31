using ScalableTeaching.Helpers;
using System;

namespace ScalableTeaching.DTO
{
    public class CreateMachineDTO
    {
        public string MachineName { get; set; }
        public Guid ClassID { get; set; }
        public ReplicationDirectives ReplicationDirective { get; set; }


    }

}
