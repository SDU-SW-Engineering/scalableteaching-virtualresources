using System;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class User
    {
        [Key]
        public String Username { get; set; }
        public String Mail { get; set; }
        [Required]
        public UserType AccountType { get; set; }
        [Required] 
        public string UserPrivateKey { get; set; }
        

        public enum UserType
        {
            User,
            Manager,
            Administrator,
        }
        
    }

    
}
