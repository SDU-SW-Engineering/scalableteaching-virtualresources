using System;

namespace ScalableTeaching.DTO
{
    public class ConnectionStringSingleDTO
    {
        public string Username { get; set; }
        public string ConnectionString { get; set; }
        public Guid MachineID { get; set; }
    }
}
