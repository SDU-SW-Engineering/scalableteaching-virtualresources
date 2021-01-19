using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using backend.DTO;
using backend.Helpers;
using Microsoft.Extensions.Configuration;
using backend.Data;

namespace backend.Controllers
{
    [Route("/api/login")]
    public class AuthController : ControllerBase
    {
        private readonly VmDeploymentContext context;
        private readonly IConfiguration configuration;
        public AuthController(IConfiguration configuration, VmDeploymentContext context) 
        {
            this.configuration = configuration;
            this.context = context;
        }
        // post: AuthController
        [HttpPost]
        public async Task<ActionResult> PostToken(SSOTokenDTO tokendata)
        {
            try
            {
                UserDTO user = await SSOHelper.GetSSOData(tokendata);
                user.AccountType = context.Users.Find(user.Username).AccountType;
                if(user.AccountType == null)
                {
                    context.Users.Add(new Models.User()
                    {
                        AccountType = "User",
                        Mail = user.Mail,
                        UserID = Guid.NewGuid(),
                        Username = user.Username
                    });
                    _ = await context.SaveChangesAsync();
                    user.AccountType = "User";
                }
                var response = new { jwt = JwtHelper.Create(user, configuration.GetValue<String>("APIHostName")) };
                return Ok(response);
            }
            catch (ArgumentException e)
            {
                return Unauthorized("Authentication Failed");
            }
        }

        [HttpGet]
        public ActionResult GetKey()
        {
            return Ok(CryptoHelper.Instance.GetPublicKeyPem());
        }

        //[HttpGet("refresh")]
        //public ActionResult GetRefresh()
        //{

        //}



    }
}
