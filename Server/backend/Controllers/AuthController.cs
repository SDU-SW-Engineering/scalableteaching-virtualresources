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
using System.Text.Json;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("/api/login")]
    [ApiController]    
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
            Console.WriteLine(tokendata);
            try
            {
                UserDTO user = await SSOHelper.GetSSOData(tokendata);
                User DatabaseUserReturn = context.Users.Find(user.Username);
                if (DatabaseUserReturn == null)
                {
                    context.Users.Add(new User()
                    {
                        AccountType = "User",
                        Mail = user.Mail,
                        UserID = Guid.NewGuid(),
                        Username = user.Username
                    });
                    _ = await context.SaveChangesAsync();
                    user.AccountType = "User";
                }
                else
                {
                    user.AccountType = DatabaseUserReturn.AccountType;
                }
                var response = new { jwt = JwtHelper.Create(user, configuration.GetValue<String>("APIHostName")) };
                return Ok(response);
            }
            catch (ArgumentException e)
            {
                return Unauthorized("Authentication Failed");
            }
        }

        [HttpPost("validate/user")]
        [Authorize(Policy = "UserLevel")]
        public async Task<ActionResult> PostValidateUser()
        {
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpPost("validate/manager")]
        [Authorize(Policy = "ManagerLevel")]
        public async Task<ActionResult> PostValidateManager()
        {
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpPost("validate/administrator")]
        [Authorize(Policy = "AdministratorLevel")]
        public async Task<ActionResult> PostValidateAdministrator()
        {
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpGet]
        public ActionResult GetKey()
        {
            var json = JsonSerializer.Serialize(new PubKey(CryptoHelper.Instance.GetPublicKeyPem()));
            return Ok(json);
        }
    }
}
