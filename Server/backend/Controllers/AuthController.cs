using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("/api/Login")]
    public class AuthController : Controller
    {
        // post: AuthController
        [HttpPost]
        public ActionResult index(String Token)
        {
            Console.Out.WriteLine(Token.ToString);
            return Ok("Ok");
        }

        
    }
}
