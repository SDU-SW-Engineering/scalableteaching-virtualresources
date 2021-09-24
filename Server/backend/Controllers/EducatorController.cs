using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScalableTeaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdministratorLevel")]
    public class EducatorController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public EducatorController(VmDeploymentContext context)
        {
            _context = context;
        }

        // GET: api/Educator
        // Returns all people of educator level
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EducatorReturnDTO>>> GetUsers()
        {

            return await _context.Users.Where(user => user.AccountType.Equals(Models.User.UserType.Educator))
                .Cast<EducatorReturnDTO>().ToListAsync();
        }

        // POST: api/Educator
        // Upgrades someone to educator level based on email
        [HttpPost]
        public async Task<ActionResult> PostUser(EducatorEmailDTO emailDTO)
        {
            var email = emailDTO.Email;
            if (email == null || email.Length == 0) return BadRequest("Input Empty");
            if (!(new Regex(@"^[A-Za-z0-9]{1,10}@[a-zA-Z0-9]*\.sdu\.dk$").IsMatch(email))) return BadRequest("Email was invalid");
            var foundUser = await _context.Users.FindAsync(email.Split("@")[0]);
            if (foundUser == null)
            {

                await _context.Users.AddAsync(await UserFactory.Create(Email: email, Username: email.Split("@")[0]));
                await _context.SaveChangesAsync();
                return Ok("New user created based on entered email");
            }
            else
            {
                if (foundUser.AccountType == Models.User.UserType.Administrator ||
                    foundUser.AccountType == Models.User.UserType.Educator)
                {
                    return BadRequest("Requested User allready has the requested or a higher permission level");
                }
                foundUser.AccountType = Models.User.UserType.Educator;
                _context.Users.Update(foundUser);
                await _context.SaveChangesAsync();
                return Ok("Existing user updated");
            }
        }

        // DELETE: api/Educator/5
        [HttpDelete("{email}")]
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
