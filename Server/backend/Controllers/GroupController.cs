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
using backend.DTO;

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

        // GET: api/group - Return all groups for a given user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
        {
            IEnumerable<Group> groups = await Task.Factory.StartNew< IEnumerable < Group >>(() => _context.Groups.Where(group => group.Course.User.Username == getUsername()));
            return groups.Any() ? Ok(groups) : NoContent();

        }

        // GET: api/group/5 - Return requested group by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetGroup(Guid id)
        {
            var @group = _context.Groups.Where(group => group.GroupID == id && group.Course.User.Username == getUsername()).First<Group>();

            if (@group == null)
            {
                return NotFound();
            }

            return group == null ? NotFound() : @group;
        }

        // PUT: api/group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(Guid id, GroupNameDTO groupNameDTO)
        {

            //Validate the request
            if (id != groupNameDTO.GroupID) return BadRequest();
            //Validate course
            if (!_context.Courses.Where<Course>(Course => Course.User.Username == getUsername() && Course.CouseID == groupNameDTO.CourseID).Any()) return BadRequest();

            try
            {
                var group = await _context.Groups.FindAsync(id);
                group.GroupName = groupNameDTO.GroupName;
                _context.Entry(group).State = EntityState.Modified;
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
            catch (NullReferenceException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/group
        [HttpPost]
        public async Task<ActionResult<Group>> PostGroup(GroupDTO GroupDTO)
        {
            //Validate the course
            if(!_context.Courses.Where(Course => Course.User.Username == getUsername() && Course.CouseID == GroupDTO.CourseID).Any()) return BadRequest();
            //Validate the group
            if (_context.Groups.Any(Group => Group.GroupName == GroupDTO.GroupName))
            {
                return Conflict();
            }
            else
            {
                var group = new Group()
                {
                    GroupName = GroupDTO.GroupName,
                    CourseID = GroupDTO.CourseID,
                    GroupID = Guid.NewGuid()  
                };
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetGroup", new { id = @group.GroupID }, @group);
            }
        }

        // DELETE: api/group/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            var @group = _context.Groups.Where(Group => Group.Course.User.Username == getUsername() && Group.GroupID == id).First();
            if (@group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GroupExists(Guid id)
        {
            return _context.Groups.Any(e => e.GroupID == id);
        }

        public string getUsername()
        {
            ////var TokenUser = JwtHelper.Decode(await HttpContext.GetTokenAsync("Bearer", "access_token"));
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
    }
}
