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
using Microsoft.AspNetCore.Razor.Language;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ManagerLevel")]
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
            if (! await _context.Groups.AnyAsync(group => group.Course.User.Username == getUsername() && group.GroupID == groupId)) return BadRequest("You are no associated with any group with this id");
            var assignments = _context.GroupAssignments.Where(assignment => assignment.GroupID == groupId).ToListAsync();
            var outputStructure = new { GroupID = groupId, Members = new List<string>() };
            foreach(var assignment in await assignments)
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
            if (!_context.Groups.Any(group => group.GroupID == dto.GroupID &&
                                              group.Course.User.Username == getUsername())) 
            { 
                return BadRequest("Either group does not exist or the group is not on a course owned by you"); 
            }

            await _context.GroupAssignments.AddAsync(new GroupAssignment() { GroupID=dto.GroupID,UserUsername=dto.UserUsername});
            await _context.SaveChangesAsync();
            // ReSharper disable once Mvc.ActionNotResolved
            return CreatedAtAction("members", new {groupid = dto.GroupID});
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
                                              @group.Course.User.Username == getUsername())
            )
            {
                return BadRequest("Either group does not exist or the group is not on a course owned by you");
            }

            _context.GroupAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        
        
        
        private bool GroupAssignmentExists(Guid id)
        {
            return _context.GroupAssignments.Any(e => e.GroupID == id);
        }

        private string getUsername()
        {
            return HttpContext.User.Claims.Where(claim => claim.Type == "username").First().Value;
        }
        
    }
}
