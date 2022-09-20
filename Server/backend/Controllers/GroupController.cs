using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScalableTeaching.Controllers.Extensions;
using Serilog;

namespace ScalableTeaching.Controllers
{
    [Route("api/group")]
    [ApiController]
    [Authorize(Policy = "EducatorLevel")]
    public class GroupController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public GroupController(VmDeploymentContext context)
        {
            _context = context;
        }

        // GET: api/group - Return all groups for a given user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupOutDTO>>> GetGroups()
        {
            var returnList = await _context.Groups.Where(group => group.Course.UserUsername == this.GetUsername()).Cast<GroupOutDTO>().ToListAsync();
            return returnList.Count > 0 ? Ok(returnList) : NoContent();
        }

        // GET: api/group - Return all groups for a given user
        [HttpGet("course/{id}")]
        public async Task<ActionResult<IEnumerable<GroupOutDTO>>> GetGroups(Guid id)
        {
            var returnList = await _context.Groups.Where(group => group.Course.UserUsername == this.GetUsername() && group.CourseID == id).Cast<GroupOutDTO>().ToListAsync();
            return returnList.Count > 0 ? Ok(returnList) : NoContent();
        }

        // GET: api/group/5 - Return requested group by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetGroup(Guid id)
        {
            var foundGroup = await _context.Groups.Where(group => group.GroupID == id && group.Course.User.Username == this.GetUsername()).FirstAsync();

#pragma warning disable IDE0029 // Use coalesce expression - Suppressed due to ignoronius prompt. Line is incompatible with the operator.
            return foundGroup == null ? NotFound() : foundGroup;
#pragma warning restore IDE0029 // Use coalesce expression
        }

        // PUT: api/group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(Guid id, GroupNameDTO dto)
        {

            //Validate the request
            if (id != dto.GroupID) return BadRequest();
            //Validate course
            if (!_context.Courses.Any(course => course.User.Username == this.GetUsername() && course.CourseID == dto.CourseID)) return BadRequest();

            try
            {
                var group = await _context.Groups.FindAsync(id);
                group.GroupName = dto.GroupName;
                _context.Groups.Update(group);
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
        public async Task<ActionResult<GroupOutDTO>> PostGroup(GroupDTO dto)
        {
            //Validate the course
            if (!_context.Courses.Any(course => course.User.Username == this.GetUsername() && course.CourseID == dto.CourseID)) return BadRequest();
            //Validate the group
            if (_context.Groups.Any(group => group.GroupName == dto.GroupName))
            {
                return Conflict();
            }
            else
            {
                var groupsInCourse = await _context.Groups.Where(group => group.CourseID == dto.CourseID).ToListAsync();
                int maxIndex = 0;
                if (groupsInCourse.Count >= 1) maxIndex = groupsInCourse.Max((group) => { return group.GroupIndex; });

                var group = new Group()
                {
                    GroupName = dto.GroupName,
                    CourseID = dto.CourseID,
                    GroupID = Guid.NewGuid(),
                    GroupIndex = maxIndex + 1
                };
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetGroup", new { id = @group.GroupID }, @group);
            }
        }

        // DELETE: api/group/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            var foundGroup = await _context.Groups.FirstAsync(group => group.Course.User.Username == this.GetUsername() && group.GroupID == id);
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
            if (course is null || course.User.Username != this.GetUsername()) return BadRequest();
            //Create Group
            var group = new Group() { CourseID = dto.CourseID, GroupID = Guid.NewGuid(), GroupName = dto.GroupName };
            //Create task for adding group to optimise code
            _context.Groups.Add(group);

            Log.Debug("Filled group dto {Dto}", dto);
            
            //Ensure that all incoming usernames are in lowercase
            
            var dtoUsers = dto.Users.AsParallel().Select(u => u.ToLower()).ToList();
            
            //Create list for the assignment of users
            var groupAssignments = new List<GroupAssignment>();
            
            //Users already existing in system
            var existingUsers = 
                await _context.Users.Where(user => dtoUsers.Contains(user.Username)).ToListAsync();
            Log.Debug("Existing users: {{{Users}}}",
            string.Join(",", existingUsers.Select(eu => eu.Username)));
            
            //Create group assignment objects for already existing users
            //and remove them from the input dto to use for sorting for non existent users
            foreach (var user in existingUsers)
            {
                groupAssignments.Add(new GroupAssignment() { GroupID = group.GroupID, UserUsername = user.Username });
                dtoUsers.Remove(user.Username);
            }

            //create users that did not already exist in the system
            var usersToBeCreated = new List<User>();
            foreach (var username in dtoUsers)
            {
                usersToBeCreated.Add(await UserFactory.Create(username));
            }

            //Await group creation and the adding of users, and save these data points
            _context.Users.AddRange(usersToBeCreated);
            await _context.SaveChangesAsync();

            //Create group assignments for newly created users
            groupAssignments.AddRange(dtoUsers.Select(username => new GroupAssignment() {GroupID = group.GroupID, UserUsername = username}));

            //Add all assignments to the database
            _context.GroupAssignments.AddRange(groupAssignments);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroup), new { id = group.GroupID }, (GroupOutDTO)group);
        }

        private bool GroupExists(Guid id)
        {
            return _context.Groups.Any(e => e.GroupID == id);
        }
    }
}
