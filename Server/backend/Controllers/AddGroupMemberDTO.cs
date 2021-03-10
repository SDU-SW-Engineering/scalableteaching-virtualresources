using System;

namespace Backend.Controllers
{
    public class AddGroupMemberDTO
    {
        public string UserUsername { get; set; }
        public Guid GroupID { get; set; }
    }
}