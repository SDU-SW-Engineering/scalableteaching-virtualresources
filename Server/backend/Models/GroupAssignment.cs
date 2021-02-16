using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class GroupAssignment
    {
        public Guid GroupID { get; set; }
        public Guid UserID { get; set; }

        public virtual Group Group { get; set; }
        public virtual User User { get; set; }

    }
}
