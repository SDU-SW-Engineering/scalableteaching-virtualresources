using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ScalableTeaching.Controllers.Extensions;
using Serilog;

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
            Log.Verbose("AuthController-PostToken-Tokendata:{tokendata}",tokendata);
            try
            {
                UserDTO user = await SSOHelper.GetSSOData(tokendata);
                User databaseUserReturn = await _context.Users.FindAsync(user.Username.ToLower());
                if (databaseUserReturn == null)
                {
                    Log.Verbose("AuthController-PostToken-new user: {username}", user.Username);
                    _context.Users.Add(await UserFactory.Create(user.Username, user.Mail, user.Gn, user.Sn));
                    await _context.SaveChangesAsync();
                    user.AccountType = nameof(Models.User.UserType.User);
                }
                else
                {
                    Log.Verbose("AuthController-PostToken-Existing user: {username}, Account type: {usertype}", user.Username, databaseUserReturn.AccountType.ToString());
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
                Log.Verbose("Authcontroller-PostToken-Authentication Failed");
                return Unauthorized("Authentication Failed");
            }
        }

        [HttpPost("validate/user")]
        [Authorize(Policy = "UserLevel")]
        public ActionResult PostValidateUser()
        {
            Log.Verbose("AuthController-PostValidateUser-UserValidatedAsUser: {Username}", this.GetUsername());
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpPost("validate/educator")]
        [Authorize(Policy = "EducatorLevel")]
        public ActionResult PostValidateEducator()
        {
            Log.Verbose("AuthController-PostValidateUser-UserValidatedAsEducator: {Username}", this.GetUsername());
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        [HttpPost("validate/administrator")]
        [Authorize(Policy = "AdministratorLevel")]
        public ActionResult PostValidateAdministrator()
        {
            Log.Verbose("AuthController-PostValidateUser-UserValidatedAsAdministrator: {Username}", this.GetUsername());
            return Ok("Valid - Your credentials are valid for this level of access");
        }

        /// <summary>
        /// Get the public key for the Json Web Token
        /// </summary>
        /// <returns>Json serialised public key with a code 200</returns>
        [HttpGet]
        public ActionResult GetKey()
        {
            Log.Verbose("AuthController-GetKey: Someone requested the public key for jwt authentication");
            var json = JsonSerializer.Serialize(new PubKey(CryptoHelper.Instance.GetPublicKeyPem()));
            return Ok(json);
        }
    }
}
