using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class User
    {
        [Key]
        public string Username { get; set; }
        public string Surname { get; set; }
        public string GeneralName { get; set; }
        public string Mail { get; set; }
        [Required]
        public UserType AccountType { get; set; }
        [Required]
        public string UserPrivateKey { get; set; }
        [Required]
        public string UserPublicKey { get; set; }
        public virtual List<GroupAssignment> GroupAssignments { get; set; }
        public virtual List<MachineAssignment> MachineAssignments { get; set; }


        public enum UserType
        {
            User,
            Educator,
            Administrator,
        }

    }


}
