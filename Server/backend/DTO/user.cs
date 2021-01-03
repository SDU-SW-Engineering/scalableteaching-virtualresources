using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.DTO
{
    public class user
    {
        [Key]
        public String username { get; set; 
    }
}
