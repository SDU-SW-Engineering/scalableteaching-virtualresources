using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupAssignmentController : ControllerBase
    {
        private readonly VmDeploymentContext _context;

        public GroupAssignmentController(VmDeploymentContext context)
        {
            _context = context;
        }

        [HttpGet("members/{groupid}")]
        public async Task<ActionResult> GetGroupMembers(Guid groupID)
        {
            if (!_context.Groups.Where(group => group.Course.User.Username == getUsername() && group.GroupID == groupID).Any()) return BadRequest("You are no associated with any group with this id");
            var assignments = _context.GroupAssignments.Where(assignment => assignment.GroupID == groupID).ToList();
            var outputStructure = new { GroupID = groupID, Members = new List<string>() };
            foreach(var assignment in assignments)
            {
                outputStructure.Members.Add(assignment.UserUsername);
            }
            return Ok(outputStructure);
        }

        [HttpPost("assign")]
        public async Task<ActionResult> AddMemberToGroup(AddGroupMemberDTO dto)
        {
            //Validate DTO
            if(dto.GroupID == Guid.Empty || dto.UserUsername is null || dto.UserUsername.Any()) return BadRequest("All fields must contain valid values");
            //Prevent duplicate
            if((await _context.GroupAssignments.FindAsync(dto.GroupID, dto.UserUsername)) is not null)
            {
                return Conflict("Assignment already exists");
            }
            if (
                !_context.Groups.Where(group =>
                    group.GroupID == dto.GroupID &&
                    group.Course.User.Username == getUsername()).Any()
            ) 
            { 
                return BadRequest("Either group does not exist or the group is not on a course owned by you"); 
            }

            await _context.GroupAssignments.AddAsync(new GroupAssignment() { GroupID=dto.GroupID,UserUsername=dto.UserUsername});
            await _context.SaveChangesAsync();
            return CreatedAtAction("assign", new {groupid = dto.GroupID});
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
