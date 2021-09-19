using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.DTO
{
    public class ConnectionStringSingleDTO
    {
        public string Username { get; set; }
        public string ConnectionString { get; set; }
        public Guid MachineID { get; set; }
    }
}
