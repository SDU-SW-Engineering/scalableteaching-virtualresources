using System;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.DTO
{
    public class LoginDTO
    {
        [Required]
        public String Token { get; set; }
    }
}
