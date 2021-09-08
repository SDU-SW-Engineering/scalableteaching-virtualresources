using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Models;
using ScalableTeaching.OpenNebula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScalableTeaching.Services;
using ScalableTeaching.Helpers;

namespace ScalableTeaching.Controllers
{
    [Authorize(Policy = "UserLevel")]
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        private readonly VmDeploymentContext _context;
        private readonly IOpenNebulaAccessor _accessor;
        private readonly MachineControllerService _MachineControllerService;
        private readonly SshConfigBuilder _sshConfigBuilder;
        private readonly string ValidateAptRegex = @"^[0-9A-Za-z.+-]$";
        private readonly string ValidatePpaRegex = @"^(ppa:([a-z-]+)\/[a-z-]+)$";
        private readonly string ValidateLinuxGroup = @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$";
        private readonly string ValidateHostname = @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$";
        private readonly string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz1234567890.,-_!?";

        public MachineController(VmDeploymentContext context, IOpenNebulaAccessor accessor, MachineControllerService machineControllerService, SshConfigBuilder sshConfigBuilder)
        {
            _context = context;
            _accessor = accessor;
            _MachineControllerService = machineControllerService;
            _sshConfigBuilder = sshConfigBuilder;
        }

        [Authorize(Policy = "UserLevel")]
        [HttpGet]
        public async Task<ActionResult<List<MachineManagementReturn>>> GetAvailableMachines()//TODO: Might suffer EF issues
        {
            //TODO: Fix machine assignement stuff
            List<Machine> machines = _context.Machines.Where(machine => machine.UserUsername == GetUsername()).ToList();
            _context.MachineAssignments.Where(assignment => assignment.UserUsername == GetUsername()).ToList().ForEach(assignment => machines.Add(assignment.Machine));
            List<MachineManagementReturn> returnList = new();

            foreach (Machine machine in machines)
            {
                List<string> usernames = new();
                usernames.Add(machine.UserUsername);
                machine.MachineAssignments.ForEach(assignment =>
                {
                    if (assignment.UserUsername == null)
                    {
                        assignment.Group.GroupAssignments.ForEach(gassignment => usernames.Add(gassignment.UserUsername));
                    }
                });
                returnList.Add(new MachineManagementReturn()
                {
                    Course = (CourseDTO)machine.Course,
                    Hostname = machine.HostName,
                    IpAddress = machine.MachineStatus.MachineIp,
                    MacAddress = machine.MachineStatus.MachineIp,
                    MachineID = machine.MachineID,
                    Ports = machine.Ports,
                    Status = machine.MachineStatus.MachineState.ToString(),
                    Users = usernames
                });
            }
            return returnList;
        }

        [HttpPost]
        [Route("control/reboot/{id}")]
        public async Task<ActionResult> PostRebootMachine(Guid id)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            //Validate machine existance
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return BadRequest("Machine Not Found");
            //Validate machine "ownership"
            if (!machine.MachineAssignments.Where(assignment => assignment.User.Username == GetUsername()).Any() && machine.UserUsername != GetUsername()) return BadRequest("You are note assigned to this machine");//TODO: Fix machine assignement stuff
            //Reboot Machine
            if (_accessor.PerformVirtualMachineAction(MachineActions.REBOOT, (int)machine.OpenNebulaID)) return Ok();
            else return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [Authorize(Policy = "UserLevel")]
        [HttpPost]
        [Route("control/reboot-hard/{id}")]
        public async Task<ActionResult> PostRebootMachineHard(Guid id)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            //Validate machine existance
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return BadRequest("Machine Not Found");
            //Validate machine "ownership"
            if (!machine.MachineAssignments.Where(assignment => assignment.User.Username == GetUsername()).Any() && machine.UserUsername != GetUsername()) return BadRequest("You are note assigned to this machine");//TODO: Fix machine assignement stuff
            //Reboot Machine
            if (_accessor.PerformVirtualMachineAction(MachineActions.REBOOT_HARD, (int)machine.OpenNebulaID)) return Ok();
            else return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [Authorize(Policy = "UserLevel")]
        [HttpDelete]
        [Route("control/delete/id")]
        public async Task<ActionResult> DeleteMachine(Guid id)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            //Validate machine existance
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return BadRequest("Machine Not Found");
            //Validate machine "ownership"
            if (machine.UserUsername != GetUsername()) return BadRequest("You do not own this machine");
            //Schedule for deletion
            machine.MachineCreationStatus = CreationStatus.SHEDULED_FOR_DELETION;
            _context.Machines.Update(machine);
            await _context.SaveChangesAsync();
            var deletionTime = _MachineControllerService.ScheduleForDeletion(machine);
            return Ok($"Machine scheduled for deletion: {deletionTime.ToLocalTime()}");
        }

        // POST: api/machine/create/groupbased
        [Authorize(Policy = "EducatorLevel")]
        [HttpPost("create/groupbased")]
        public async Task<ActionResult<Machine>> PostMachineGroupBased(CreateMachineGroupBased machines)
        {

            var Randomizer = new Random();
            //Validate input
            foreach (var machine in machines.Machines)
            {
                //Validate ownership
                var group = await _context.Groups.FindAsync(machine.OwningGroup);
                if (group == null) return BadRequest($"Invalid Group id");
                if (group.CourseID != machine.CourseID) return BadRequest($"GroupID: {machine.OwningGroup} is not assiciated with the course {machine.CourseID}");
                if (group.Course.UserUsername != GetUsername()) return Unauthorized($"You do not have ownership over the course: {machine.CourseID}, and therefor not over the group id requested");

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
                    MachineCreationStatus = CreationStatus.REGISTERED,
                    Apt = machine.Apt,
                    LinuxGroups = machine.LinuxGroups,
                    Ports = machine.Ports,
                    Ppa = machine.Ppa
                });
                _context.MachineAssignments.Add(new()
                {
                    MachineAssignmentID = Guid.NewGuid(),
                    GroupID = machine.OwningGroup,
                    MachineID = NewMachineID,
                    OneTimePassword = new string(Enumerable.Repeat(chars, 12).Select(s => s[Randomizer.Next(s.Length)]).ToArray()),
                    UserUsername = null
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Machines Registered for creation, creation will start soon.");
        }

        // POST: api/machine/create/groupbased
        [Authorize(Policy = "EducatorLevel")]
        [HttpPost("create/userbased")]
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
                    MachineAssignmentID = Guid.NewGuid(),
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
            return HttpContext.User.Claims.First(claim => claim.Type == "username").Value.ToLower();
        }
    }
}
