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
using Backend.DTO;

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

        [HttpPost("member/group")]
        public async Task<IActionResult> PostEntireGroup(FilledGroupDTO filledGroup)
        {
            //Ensure that group does not exist
            if (_context.Groups.Where(group => group.CourseID == filledGroup.CourseId && group.GroupName == filledGroup.GroupName).Any()) return Conflict();
            var course = await _context.Courses.FindAsync(filledGroup.CourseId);
            //Ensure that course is valid and assigned to requesting user
            if (course is null || course.User.Username != getUsername()) return BadRequest();
            //Create Group
            var group = new Group() { CourseID = filledGroup.CourseId, GroupID = Guid.NewGuid(), GroupName = filledGroup.GroupName };
            //Create task for adding group to optimise code
            var groupAddingTask = _context.Groups.AddAsync(group);


            //Create list for the assignment of users
            var groupAssignments = new List<GroupAssignment>();
            //Users allready existing in system
            var existingUsers = _context.Users.Where(user => filledGroup.Users.Contains(user.Username)).AsEnumerable();

            //Create group assignment objects for allready existing users and remove them from the input dto to use for sorting for non existant users
            foreach (var user in existingUsers)
            {
                groupAssignments.Add(new GroupAssignment() { GroupID = group.GroupID, UserUsername = user.Username });
                filledGroup.Users.Remove(user.Username);
            }

            //create users that did not allready exist in the system
            var usersToBeCreated = new List<User>();
            foreach (string username in filledGroup.Users)
            {
                usersToBeCreated.Add(new User() { AccountType = Models.User.UserType.User });
            }

            //Await group creation and the adding of users, and save these data points
            await groupAddingTask;
            await _context.Users.AddRangeAsync(usersToBeCreated);
            await _context.SaveChangesAsync();

            //Create group assignments for newly created users
            foreach (string username in filledGroup.Users)
            {
                groupAssignments.Add(new GroupAssignment() { GroupID = group.GroupID, UserUsername = username });
            }

            //Add all assignments to the database
            await _context.GroupAssignments.AddRangeAsync(groupAssignments);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGroup", new { id = @group.GroupID }, new { group = group, assignments = groupAssignments });
        }

        private bool GroupExists(Guid id)
        {
            return _context.Groups.Any(e => e.GroupID == id);
        }

        private string getUsername()
        {
            ////var TokenUser = JwtHelper.Decode(await HttpContext.GetTokenAsync("Bearer", "access_token"));
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
    }
}
