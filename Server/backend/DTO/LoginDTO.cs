using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    public class LoginDTO
    {
        [Required]
        String Token { get; set; }
    }
}
