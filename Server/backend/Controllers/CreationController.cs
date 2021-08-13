using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Models;

namespace ScalableTeaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreationController : ControllerBase
    {
        private string ValidateAptRegex = @"^[0-9A-Za-z.+-]$";
        private string ValidatePpaRegex = @"^(ppa:([a-z-]+)\/[a-z-]+)$";
        private string ValidateLinuxGroup = @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$";
        private string ValidateHostname = @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$";
        private string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz1234567890.,-_!?";

        private readonly VmDeploymentContext _context;

        public CreationController(VmDeploymentContext context)
        {
            _context = context;
        }

        // POST: api/Creation
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("groupbased")]
        public async Task<ActionResult<Machine>> PostMachineGroupBased(CreateMachineGroupBased machines)
        {

            var Randomizer = new Random();
            //Validate input
            foreach(var machine in machines.Machines)
            {
                //Validate ownership
                var group = await _context.Groups.FindAsync(machine.OwningGroup);
                if (group == null) return BadRequest($"Invalid Group id");
                if (group.CourseID != machine.CourseID) return BadRequest($"GroupID: {machine.OwningGroup} is not assiciated with the course {machine.CourseID}");
                if (group.Course.UserUsername != GetUsername()) return Unauthorized($"You do not have ownership over the course: {machine.CourseID}, and therefor not over the group id requested");

                //Validate Content
                if (!Regex.IsMatch(machine.Hostname, ValidateHostname)) return BadRequest($"Invalid Hostname: {machine.Hostname}");
                if (!machine.Apt.AsParallel().All(apt => Regex.IsMatch(apt, ValidateAptRegex))) return BadRequest($"Invalid apt package in list: {String.Join(", ",machine.Apt.ToArray())}");
                if (!machine.Ppa.AsParallel().All(ppa => Regex.IsMatch(ppa, ValidatePpaRegex))) return BadRequest($"Invalid ppa in list: {String.Join(", ", machine.Ppa.ToArray())}");
                if (!machine.LinuxGroups.AsParallel().All(group => Regex.IsMatch(group, ValidateLinuxGroup))) return BadRequest($"Invalid linux group in list: {String.Join(", ", machine.LinuxGroups)}");
                if (!machine.Ports.AsParallel().All(port => port > 0 && port <= 65535)) return BadRequest($"Port Out of bound in list: {String.Join(", ", machine.Ports)}");
                var NewMachineID = Guid.NewGuid();
                _context.Machines.Add(new Machine
                {
                    CourseID = machine.CourseID,
                    HostName = machine.Hostname,
                    MachineID = NewMachineID,
                    UserUsername = GetUsername(),
                    MachineCreationStatus = CreationStatus.REGISTERED                    
                });
                _context.MachineAssignments.Add(new()
                {
                    GroupID = machine.OwningGroup,
                    MachineID = NewMachineID,
                    OneTimePassword = new string(Enumerable.Repeat(chars, 12).Select(s=>s[Randomizer.Next(s.Length)]).ToArray()),
                    UserUsername = null
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Machines Registered for creation, creation will start soon.");
        }

        // POST: api/Creation
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("userbased")]
        public async Task<ActionResult<Machine>> PostMachineUserBased(CreateMachineUsersBased machines)
        {

            var Randomizer = new Random();
            //Validate input
            foreach (var machine in machines.Machines)
            {
                //Validate ownership
                var course = await _context.Courses.FindAsync(machine.CourseID);
                if (course == null) return BadRequest($"The requested course id:({machine.CourseID}) does not exists");
                if (course.UserUsername != GetUsername()) return Unauthorized($"You do not own the course({course.CourseID})");

                //Validate Content
                if (!Regex.IsMatch(machine.Hostname, ValidateHostname)) return BadRequest($"Invalid Hostname: {machine.Hostname}");
                if (!machine.Apt.AsParallel().All(apt => Regex.IsMatch(apt, ValidateAptRegex))) return BadRequest($"Invalid apt package in list: {String.Join(", ", machine.Apt.ToArray())}");
                if (!machine.Ppa.AsParallel().All(ppa => Regex.IsMatch(ppa, ValidatePpaRegex))) return BadRequest($"Invalid ppa in list: {String.Join(", ", machine.Ppa.ToArray())}");
                if (!machine.LinuxGroups.AsParallel().All(group => Regex.IsMatch(group, ValidateLinuxGroup))) return BadRequest($"Invalid linux group in list: {String.Join(", ", machine.LinuxGroups)}");
                if (!machine.Ports.AsParallel().All(port => port > 0 && port <= 65535)) return BadRequest($"Port Out of bound in list: {String.Join(", ", machine.Ports)}");
                var NewMachineID = Guid.NewGuid();
                _context.Machines.Add(new Machine
                {
                    CourseID = machine.CourseID,
                    HostName = machine.Hostname,
                    MachineID = NewMachineID,
                    UserUsername = GetUsername(),
                    MachineCreationStatus = CreationStatus.REGISTERED
                });
                _context.MachineAssignments.Add(new()
                {
                    GroupID = null,
                    MachineID = NewMachineID,
                    OneTimePassword = new string(Enumerable.Repeat(chars, 12).Select(s => s[Randomizer.Next(s.Length)]).ToArray()),
                    UserUsername = null
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Machines Registered for creation, creation will start soon.");
        }

        /// <summary>
        /// Gives the value of the username claim for the current httpcontext
        /// </summary>
        /// <returns>Username of logged in user</returns>
        private string GetUsername()
        {
            return HttpContext.User.Claims.First(claim => claim.Type == "username").Value;
        }
    }
}
