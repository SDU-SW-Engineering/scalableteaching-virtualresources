using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScalableTeaching.Controllers.Extensions;

namespace ScalableTeaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "EducatorLevel")]
    public class GroupAssignmentController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public GroupAssignmentController(VmDeploymentContext context)
        {
            _context = context;
        }

        [HttpGet("{groupid}")]
        public async Task<ActionResult> GetGroupMembers(Guid groupId)
        {
            if (!await _context.Groups.AnyAsync(group => group.Course.User.Username == this.GetUsername() && group.GroupID == groupId)) return BadRequest("You are no associated with any group with this id");
            var assignments = _context.GroupAssignments.Where(assignment => assignment.GroupID == groupId).ToListAsync();
            var outputStructure = new { GroupID = groupId, Members = new List<string>() };
            foreach (var assignment in await assignments)
            {
                outputStructure.Members.Add(assignment.UserUsername);
            }
            return Ok(outputStructure);
        }

        [HttpPost("assign")]
        public async Task<ActionResult> AddMemberToGroup(SingleGroupMemberDTO dto)
        {
            //Validate DTO
            if (!dto.Validate()) return BadRequest("All fields must contain valid values");
            //Prevent duplicate
            if ((await _context.GroupAssignments.FindAsync(dto.GroupID, dto.UserUsername)) is not null)
            {
                return Conflict("Assignment already exists");
            }
            if (!await _context.Groups.AnyAsync(group => group.GroupID == dto.GroupID &&
                                              group.Course.User.Username == this.GetUsername()))
            {
                return BadRequest("Either group does not exist or the group is owned by another user");
            }

            await _context.GroupAssignments
                .AddAsync(new GroupAssignment() { GroupID = dto.GroupID, UserUsername = dto.UserUsername.ToLower() });
            await _context.SaveChangesAsync();
            // ReSharper disable once Mvc.ActionNotResolved
            return CreatedAtAction("members", new { groupid = dto.GroupID });
        }

        [HttpPut("update")]
        public async Task<ActionResult> UpdateGroupMembers(AllGroupMembersDTO dto)
        {
            //Validate DTO
            if (!dto.Validate()) return BadRequest("All fields must contain valid values");


            var groupQueriable = _context.Groups.Where(
                    g => g.GroupID.Equals(dto.GroupID) && g.Course.UserUsername.Equals(this.GetUsername()));

            //Check group existance and permission
            if (!await groupQueriable.AnyAsync())
                return BadRequest("Group does not exist or is owned by different user");

            _context.GroupAssignments.RemoveRange((await groupQueriable.FirstAsync()).GroupAssignments);
            foreach (var username in dto.Usernames)
            {
                if (!await _context.Users.Where(u => u.Username == username).AnyAsync())
                {

                    await _context.Users.AddAsync(await UserFactory.Create(username));
                }
                await _context.GroupAssignments
                    .AddAsync(new GroupAssignment() { GroupID = dto.GroupID, UserUsername = username });
            }
            try
            {
                int saveResult = await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                Log.Error($"An Exception occurred whent trying to save group assignments.\n Error message: {e.Message}\n");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }

        [HttpDelete("unassign")]
        public async Task<ActionResult> RemoveMemberFromGroup(SingleGroupMemberDTO dto)
        {
            //Validate DTO
            if (!dto.Validate()) return BadRequest("All fields must contain valid values");
            //Ensure Existence
            var assignment = await _context.GroupAssignments.FindAsync(dto.GroupID, dto.UserUsername);
            if (assignment is null)
            {
                return NotFound("No Such Assignment");
            }
            if (
                !_context.Groups.Any(group => @group.GroupID == dto.GroupID &&
                                              @group.Course.User.Username == this.GetUsername())
            )
            {
                return BadRequest("Either group does not exist or the group is not on a course owned by you");
            }

            _context.GroupAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
