using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.DTO
{
    public class ConnectionStringAllDTO
    {
        public string Username { get; set; }
        public string ConnectionString { get; set; }
        public List<Guid> MachineID { get; set; }
    }
}
