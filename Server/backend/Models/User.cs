using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
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
