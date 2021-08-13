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


        public enum UserType
        {
            User,
            Educator,
            Administrator,
        }

    }


}
