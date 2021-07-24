using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.DTO
{
    public class AllGroupMembersDTO
    {
        public Guid GroupID { get; set; }
        public List<string> Usernames { get; set; }

        public bool Validate()
        {
            if (GroupID == Guid.Empty || Usernames == null || Usernames.Count == 0 ) return false;
            foreach(string user in Usernames)
            {
                if (user.Length == 0 || user.Length > 10) return false;
            }
            return true;
        }
    }
}
