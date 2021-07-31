using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScalableTeaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdministratorLevel")]
    public class ManagerController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public ManagerController(VmDeploymentContext context)
        {
            _context = context;
        }

        // GET: api/Manager
        // Returns all people of manager level
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ManagerReturnDTO>>> GetUsers()
        {

            return await _context.Users.Where(user => user.AccountType.Equals(Models.User.UserType.Manager))
                .Cast<ManagerReturnDTO>().ToListAsync();
        }

        // POST: api/Manager
        // Upgrades someone to manager level based on email
        [HttpPost]
        public async Task<ActionResult> PostUser([FromBody] string email)
        {
            if (!new Regex(@"^[A-Za-z0-9]{1,10}@[a-zA-Z0-9]*\.sdu\.dk$").IsMatch(email)) return BadRequest();
            var foundUser = await _context.Users.FindAsync(email.Split("@")[0]);
            if (foundUser == null)
            {
                await _context.Users.AddAsync(new User()
                {
                    Username = email.Split("@")[0],
                    Mail = email,
                    AccountType = Models.User.UserType.User,
                    UserPrivateKey = SSHKeyHelper.ExportKeyAsPEM(RSA.Create(2048))
                });
                return Ok("New user created based on entered email");
            }
            else
            {
                if (foundUser.AccountType == Models.User.UserType.Administrator ||
                    foundUser.AccountType == Models.User.UserType.Manager)
                {
                    return BadRequest("Requested User allready has the requested or a higher permission level");
                }
                foundUser.AccountType = Models.User.UserType.Manager;
                _context.Users.Update(foundUser);
                await _context.SaveChangesAsync();
                return Ok("Existing user updated");
            }
        }

        // DELETE: api/Manager/5
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            if (!new Regex(@"^[A-Za-z0-9]{1,10}@[a-zA-Z0-9]*\.sdu\.dk$").IsMatch(email)) return BadRequest();
            var foundUser = await _context.Users.FindAsync(email.Split("@")[0]);
            if (foundUser == null)
            {
                return NotFound();
            }
            foundUser.AccountType = Models.User.UserType.User;
            _context.Users.Update(foundUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
