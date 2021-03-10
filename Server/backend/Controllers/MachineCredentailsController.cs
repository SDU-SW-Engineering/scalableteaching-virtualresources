using backend.Data;
using backend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineCredentailsController : ControllerBase
    {
        private readonly VmDeploymentContext _context;
        private readonly SshConfigBuilder _configBuilder;

        public MachineCredentailsController(VmDeploymentContext context, SshConfigBuilder configBuilder)
        {
            _context = context;
            _configBuilder = configBuilder;
        }

        [HttpGet("{id}")]
        public IActionResult GetMachineCredential(Guid id)
        {
            var credentialQueriable = _context.MachineCredentails.Where(cred => cred.Machine.User.Username == getUsername() && cred.MachineID == id);
            if (!credentialQueriable.Any()) return NotFound();
            return Ok(_configBuilder.GetMachineCredentialStringAsync(credentialQueriable.First()));
        }//TODO: Untested

        [HttpGet]
        public IActionResult GetAllMachineCredentials()
        {
            var credentials = _context.MachineCredentails.Where(cred => cred.Machine.User.Username == getUsername()).ToList();
            var builder = new StringBuilder();
            foreach(var credential in credentials)
            {
                builder.Append(_configBuilder.GetMachineCredentialStringAsync(credential));
            }
            return Ok(builder.ToString());
        }//TODO: Untested

        private string getUsername()
        {
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
    }
}
