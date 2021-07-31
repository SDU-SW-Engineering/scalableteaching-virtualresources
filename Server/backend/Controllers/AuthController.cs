﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScalableTeaching.Controllers
{
    [Route("/api/login")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly VmDeploymentContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration, VmDeploymentContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        // post: AuthController
        [HttpPost]
        public async Task<ActionResult> PostToken(SSOTokenDTO tokendata)
        {
            Console.WriteLine(tokendata);
            try
            {
                UserDTO user = await SSOHelper.GetSSOData(tokendata);
                User databaseUserReturn = await _context.Users.FindAsync(user.Username);
                if (databaseUserReturn == null)
                {
                    _context.Users.Add(new User()
                    {
                        AccountType = Models.User.UserType.User,
                        Mail = user.Mail,
                        GeneralName = user.Gn,
                        Surname = user.Sn,
                        Username = user.Username,
                        UserPrivateKey = SSHKeyHelper.ExportKeyAsPEM(RSA.Create(2048))
                    });
                    await _context.SaveChangesAsync();
                    user.AccountType = nameof(Models.User.UserType.User);
                }
                else
                {
                    if (databaseUserReturn.Mail == null) databaseUserReturn.Mail = user.Mail;
                    if (databaseUserReturn.GeneralName == null) databaseUserReturn.GeneralName = user.Gn;
                    if (databaseUserReturn.Surname == null) databaseUserReturn.Surname = user.Sn;
                    _context.Users.Update(databaseUserReturn);
                    await _context.SaveChangesAsync();
                    user.AccountType = databaseUserReturn.AccountType.ToString();
                }

                var response = new { jwt = JwtHelper.Create(user, _configuration.GetValue<String>("APIHostName")) };
                return Ok(response);
            }
            catch (ArgumentException)
            {
                return Unauthorized("Authentication Failed");
            }
        }

        [HttpPost("validate/user")]
        [Authorize(Policy = "UserLevel")]
        public ActionResult PostValidateUser()
        {
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpPost("validate/manager")]
        [Authorize(Policy = "ManagerLevel")]
        public ActionResult PostValidateManager()
        {
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpPost("validate/administrator")]
        [Authorize(Policy = "AdministratorLevel")]
        public ActionResult PostValidateAdministrator()
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
