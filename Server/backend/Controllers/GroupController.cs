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
using System.Security.Cryptography;

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
            var groups = await Task.Factory.StartNew< IEnumerable < Group >>(() => _context.Groups.Where(group => group.Course.User.Username == GetUsername()));
            var returnList = new List<GroupOutDTO>();
            foreach (var group in groups)
            {
                returnList.Add((GroupOutDTO)group);
            } 
            return groups.Any() ? Ok(returnList) : NoContent();

        }

        // GET: api/group/5 - Return requested group by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetGroup(Guid id)
        {
            var foundGroup = await _context.Groups.Where(group => group.GroupID == id && group.Course.User.Username == GetUsername()).FirstAsync();

            return foundGroup == null ? NotFound() : foundGroup;
        }

        // PUT: api/group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(Guid id, GroupNameDTO dto)
        {

            //Validate the request
            if (id != dto.GroupID) return BadRequest();
            //Validate course
            if (!_context.Courses.Any(course => course.User.Username == GetUsername() && course.CourseID == dto.CourseID)) return BadRequest();

            try
            {
                var group = await _context.Groups.FindAsync(id);
                group.GroupName = dto.GroupName;
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
        public async Task<ActionResult<Group>> PostGroup(GroupDTO dto)
        {
            //Validate the course
            if(!_context.Courses.Any(course => course.User.Username == GetUsername() && course.CourseID == dto.CourseID)) return BadRequest();
            //Validate the group
            if (_context.Groups.Any(group => group.GroupName == dto.GroupName))
            {
                return Conflict();
            }
            else
            {
                var group = new Group()
                {
                    GroupName = dto.GroupName,
                    CourseID = dto.CourseID,
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
            var foundGroup = await _context.Groups.FirstAsync(group => group.Course.User.Username == GetUsername() && group.GroupID == id);
            if (foundGroup == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(foundGroup);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("member/group")]
        public async Task<IActionResult> PostEntireGroup(FilledGroupDTO dto)
        {
            //Ensure that group does not exist
            if (_context.Groups.Any(g => g.CourseID == dto.CourseID && g.GroupName == dto.GroupName)) return Conflict();
            var course = await _context.Courses.FindAsync(dto.CourseID);
            //Ensure that course is valid and assigned to requesting user
            if (course is null || course.User.Username != GetUsername()) return BadRequest();
            //Create Group
            var group = new Group() { CourseID = dto.CourseID, GroupID = Guid.NewGuid(), GroupName = dto.GroupName };
            //Create task for adding group to optimise code
            var groupAddingTask = _context.Groups.AddAsync(group);


            //Create list for the assignment of users
            var groupAssignments = new List<GroupAssignment>();
            //Users allready existing in system
            var existingUsers = await _context.Users.Where(user => dto.Users.Contains(user.Username)).ToListAsync();

            //Create group assignment objects for already existing users and remove them from the input dto to use for sorting for non existant users
            foreach (var user in existingUsers)
            {
                groupAssignments.Add(new GroupAssignment() { GroupID = group.GroupID, UserUsername = user.Username });
                dto.Users.Remove(user.Username);
            }

            //create users that did not allready exist in the system
            var usersToBeCreated = new List<User>();
            foreach (string username in dto.Users)
            {
                usersToBeCreated.Add(new User() { AccountType = Models.User.UserType.User, Username = username, UserPrivateKey = SSHKeyHelper.ExportKeyAsPEM(RSA.Create(2048)) });
            }

            //Await group creation and the adding of users, and save these data points
            await groupAddingTask;
            await _context.Users.AddRangeAsync(usersToBeCreated);
            await _context.SaveChangesAsync();

            //Create group assignments for newly created users
            foreach (string username in dto.Users)
            {
                groupAssignments.Add(new GroupAssignment() { GroupID = group.GroupID, UserUsername = username });
            }

            //Add all assignments to the database
            await _context.GroupAssignments.AddRangeAsync(groupAssignments);
            await _context.SaveChangesAsync();

            var returnedAssignments = new List<GroupAssignmentDTO>();
            foreach(var assignment in groupAssignments)
            {
                returnedAssignments.Add(assignment);
            }

            return CreatedAtAction(nameof(GetGroup), new { id = group.GroupID }, (GroupOutDTO)group);
        }

        private bool GroupExists(Guid id)
        {
            return _context.Groups.Any(e => e.GroupID == id);
        }

        private string GetUsername()
        {
            //Claim from the jwt token
            return HttpContext.User.Claims.First(claim => claim.Type == "username").Value;
        }
    }
}
