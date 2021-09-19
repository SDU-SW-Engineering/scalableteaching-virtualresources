using System;

namespace ScalableTeaching.Models
{
    public class GroupAssignment
    {
        public Guid GroupID { get; set; }
        public string UserUsername { get; set; }

        public virtual Group Group { get; set; }
        public virtual User User { get; set; }

    }
}
