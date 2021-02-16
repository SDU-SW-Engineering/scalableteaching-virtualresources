using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using backend.Helpers;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/group")]
    [ApiController]
    [Authorize(Policy = "ManagerLevel")]
    public class GroupController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public GroupController(VmDeploymentContext context)
        {
            _context = context;
        }

        // GET: api/group
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
        {
            IEnumerable<Group> groups = await Task.Factory.StartNew<IEnumerable<Group>>(async () => 
            {
                var TokenUser = JwtHelper.Decode(await HttpContext.GetTokenAsync("Bearer", "access_token"));
                var manager = _context.Users.Where<User>(User => User.Username == TokenUser.Username).First<User>();
                var courses = _context.Courses.Where<Course>(Course => Course.UserID == manager.UserID);
                return await _context.Group.Where<Group>(Group => 
                { 
                    foreach (var course in courses) 
                    { 
                        if(course.CouseID == Group.CourseID)
                        {
                            return true;
                        }
                    }
                    return false;
                });
            });
            return groups.Any() ? Ok(groups) : NoContent();
        }

        // GET: api/group/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetGroup(Guid id)
        {
            var @group = await Task.Factory.StartNew<Group>(async () =>
            {
                var TokenUser = JwtHelper.Decode(await HttpContext.GetTokenAsync("Bearer", "access_token"));
                var manager = _context.Users.Where<User>(User => User.Username == TokenUser.Username).First<User>();
                var foundGroup = await _context.Group.FindAsync(id);
            });

            if (@group == null)
            {
                return NotFound();
            }

            return @group;
        }

        // PUT: api/group/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(Guid id, Group @group)
        {
            if (id != @group.GroupID)
            {
                return BadRequest();
            }

            _context.Entry(@group).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/group
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Group>> PostGroup(Group @group)
        {
            _context.Group.Add(@group);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGroup", new { id = @group.GroupID }, @group);
        }

        // DELETE: api/group/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            var @group = await _context.Group.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }

            _context.Group.Remove(@group);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GroupExists(Guid id)
        {
            return _context.Group.Any(e => e.GroupID == id);
        }
    }
}
