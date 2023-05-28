using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Models;
using ScalableTeaching.OpenNebula;
using System.Text.RegularExpressions;
using Serilog;
using static ScalableTeaching.Controllers.Extensions.HttpContextExtensions;

namespace ScalableTeaching.Controllers
{
    [Authorize(Policy = "UserLevel")]
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        private readonly VmDeploymentContext _context;
        private readonly IOpenNebulaAccessor _accessor;
        private const string ValidateAptRegex = @"^[0-9A-Za-z.+-]$";
        private const string ValidatePpaRegex = @"^(ppa:([a-z-]+)\/[a-z-]+)$";

        private const string ValidateLinuxGroup =
            @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$";

        private const string ValidateHostname =
            @"^(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9-]*[A-Za-z0-9])$";

        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz1234567890.,-_!?";
        private const int MachineDeletionTiming = 7;

        public MachineController(VmDeploymentContext context, IOpenNebulaAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        /// <summary>
        /// Gets available machines.
        /// This refers to the machines that the user has created or the machines that the user has been assigned to.
        /// </summary>
        /// <returns>All available machines</returns>
        [Authorize(Policy = "UserLevel")]
        [HttpGet]
        public async Task<ActionResult<List<MachineManagementReturn>>> GetAvailableMachines()
        {
            //Get machines created by the user
            List<Machine> machines = await _context.Machines
                .Where(machine => machine.UserUsername == this.GetUsername()).ToListAsync();

            //Get machines the user has been assigned to
            machines.AddRange(await _context.MachineAssignments
                .Where(assignment => assignment.UserUsername == this.GetUsername())
                .Select(assignment => assignment.Machine)
                .ToListAsync());

            //Get all machines assigned through the groups the user is a part of
            machines.AddRange(await _context.GroupAssignments
                .Where(g => g.UserUsername == this.GetUsername())
                .SelectMany(g => g.Group.MachineAssignments)
                .Select(ma => ma.Machine).ToListAsync());

            //Remove Any duplicates
            machines = machines.DistinctBy(m => m.MachineID).ToList();

            List<MachineManagementReturn> returnList = new();
            foreach (var machine in machines)
            {
                //Create list for all the usernames that have been assigned to a machine
                List<string> usernames = new() {machine.UserUsername};

                //Get all the assignments 
                var assignments = await _context.MachineAssignments
                    .Where(assignment => assignment.MachineID == machine.MachineID).ToListAsync();
                foreach (var assignment in assignments.Where(assignment => assignment.UserUsername == null))
                {
                    usernames.AddRange(await _context.GroupAssignments
                        .Where(assign => assign.GroupID == assignment.GroupID)
                        .Select(ass => ass.UserUsername).ToListAsync());
                }

                //Construct status return value
                var returnStatus = machine.MachineStatus?.MachineState.ToString();
                if (await _context.MachineDeletionRequests.AnyAsync(request => request.MachineID == machine.MachineID))
                    returnStatus = "Scheduled for deletion";
                else if (returnStatus == null) returnStatus = "Unconfigured";
                else if (returnStatus == "DONE") returnStatus = "Deleted";

                returnList.Add(new MachineManagementReturn()
                {
                    Course = (CourseDTO) machine.Course,
                    Hostname = machine.HostName,
                    IpAddress = machine.MachineStatus?.MachineIp
                                ?? (returnStatus == "Unconfigured" ? "Configuring" : returnStatus),
                    MacAddress = machine.MachineStatus?.MachineMac
                                 ?? (returnStatus == "Unconfigured" ? "Configuring" : returnStatus),
                    MachineID = machine.MachineID,
                    Ports = machine.Ports,
                    Status = returnStatus, //machine.MachineStatus?.MachineState.ToString() ?? "Unconfigured",
                    Users = usernames,
                    Size = machine.Storage
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
            //Validate machine existence
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return BadRequest("Machine Not Found");
            //Validate machine "ownership"
            if (machine.MachineAssignments.All(assignment => assignment.UserUsername != this.GetUsername())
                && machine.UserUsername != this.GetUsername())
            {
                return BadRequest("You are note assigned to this machine");
            }

            //Reboot Machine
            // ReSharper disable once PossibleInvalidOperationException
            var returnValue = _accessor.PerformVirtualMachineAction(MachineActions.REBOOT, (int) machine.OpenNebulaID);
            //If the machine is off, then a reboot will not work, so we try to start it
            if (!returnValue)
            {
                returnValue = _accessor.PerformVirtualMachineAction(MachineActions.RESUME, (int) machine.OpenNebulaID);
            }
            
            return returnValue ? Ok("Machine reboot initialised") : StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Do a forced "power cycle" on the machine to reboot a machine that is not willingly shutting down.
        /// </summary>
        /// <param name="id">The machine to be restarted</param>
        /// <returns>
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>Status Codes</term>
        ///     </listheader>
        ///     <item>
        ///         <term>200 -</term>
        ///         <description> Machine reboot initialised</description>
        ///     </item>
        ///     <item>
        ///         <term>400 -</term>
        ///         <description> Invalid ID</description>
        ///     </item>
        ///     <item>
        ///         <term>403 -</term>
        ///         <description> You are not assigned to this machine</description>
        ///     </item>
        ///     <item>
        ///         <term>404 -</term>
        ///         <description> Machine Not Found</description>
        ///     </item>
        /// </list>
        /// </returns>
        [Authorize(Policy = "UserLevel")]
        [HttpPost]
        [Route("control/reboot-hard/{id}")]
        public async Task<ActionResult> PostRebootMachineHard(Guid id)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            //Validate machine existence
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound("Machine Not Found");
            //Validate machine "ownership"
            if (machine.MachineAssignments.All(assignment => assignment.UserUsername != this.GetUsername())
                && machine.UserUsername != this.GetUsername())
            {
                return Forbid("You are not assigned to this machine");
            }

            var returnValue =
                _accessor.PerformVirtualMachineAction(MachineActions.REBOOT_HARD, (int) machine.OpenNebulaID);
            //If the machine is off, then a reboot will not work, so we try to start it
            if (!returnValue)
            {
                returnValue = _accessor.PerformVirtualMachineAction(MachineActions.RESUME, (int) machine.OpenNebulaID);
            }
            
            return returnValue ? Ok("Machine reboot initialised") : StatusCode(StatusCodes.Status500InternalServerError);
        }

        [Authorize(Policy = "EducatorLevel")]
        [HttpPatch]
        [Route("control/resize/{id}")]
        public async Task<ActionResult> PatchResizeMachine(Guid id, int bytes)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            
            //Validate machine existence
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound("Machine Not Found");
            
            //Validate new size
            if (bytes < 0) return BadRequest("Invalid size");
            if (bytes <= machine.Storage) return BadRequest("New size must be larger than current size");
            
            //Validate machine creation status
            if (machine.MachineCreationStatus != CreationStatus.CONFIGURED)
                return BadRequest("Machine must be configured before resizing");
            
            //Validate machine "ownership"
            bool isOwner = machine.UserUsername != this.GetUsername();
            if (isOwner && !IsAdmin())
            {
                return Forbid("You are not assigned to this machine");
            }
            
            //Make call to set new size
            var returnValue = _accessor.ResizeVirtualMachine((int) machine.OpenNebulaID, bytes);
            
            //If successful, update database
            if(!returnValue.Item1)
            {
                Console.WriteLine($"Failed to resize machine({id}), old size {machine.Storage}, new size {bytes}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        
            machine.Storage = bytes;
            _context.Update(machine);
            await _context.SaveChangesAsync();
            
            //Return result
            return Ok("Machine resize completed, restart the machine for the change to take effect");
        }

        private bool IsAdmin()
        {
            var username = this.GetUsername();
            var user = _context.Users.First(user => user.Username == username);
            return user.AccountType == Models.User.UserType.Administrator;
        }

        /// <summary>
        /// Schedules a machine for deletion. This deletion will happen in the number of days specified by <see cref="MachineDeletionTiming">MachineDeletionTiming</see>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "EducatorLevel")]
        [HttpDelete]
        [Route("control/delete/{id}")]
        public async Task<ActionResult> DeleteMachine(Guid id)
        {
            //Validate id is not en empty id
            if (id == Guid.Empty) return BadRequest("Invalid ID");

            //Validate machine existence
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return BadRequest("Machine Not Found");

            //Validate machine "ownership"
            if (machine.UserUsername != this.GetUsername()) return BadRequest("You do not own this machine");

            //Validate machine not already scheduled for deletion
            {
                var existingRequest = await _context.MachineDeletionRequests.FindAsync(machine.MachineID);

                if (existingRequest != null)
                {
                    return BadRequest(
                        $"Machine already scheduled for deletion on {existingRequest.DeletionDate.ToUniversalTime()} UTC");
                }
            }

            //Schedule for deletion
            var deletionTime = DateTime.UtcNow.AddDays(MachineDeletionTiming);
            _context.MachineDeletionRequests.Add(new MachineDeletionRequest()
            {
                MachineID = machine.MachineID, //To specify machine
                DeletionDate = deletionTime, //To make deletion cancellable  
                UserUsername = this.GetUsername() //To identify deleter
            });
            machine.MachineCreationStatus = CreationStatus.SHEDULED_FOR_DELETION;
            _context.Machines.Update(machine);
            await _context.SaveChangesAsync();

            //Return on successfully
            return Ok($"Machine scheduled for deletion: {deletionTime.ToUniversalTime()} UTC");
        }

        /// <summary>
        /// Removes a machine deletion request, resulting in a machine no longer being scheduled for deletion
        /// </summary>
        /// <param name="id">The id of the machine being deleted</param>
        /// <returns>200 - Machine(id) is no longer scheduled for deletion</returns>
        [Authorize(Policy = "EducatorLevel")]
        [HttpPatch]
        [Route("control/undo_delete/{id}")]
        public async Task<ActionResult> PatchDeleteMachine(Guid id)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            //Validate machine existence
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound("Machine Not Found");
            //Validate machine "ownership"
            if (machine.UserUsername != this.GetUsername()) return Unauthorized("You do not own this machine");
            //Validate that a deletion request exists
            if (!await _context.MachineDeletionRequests.AnyAsync(request => request.MachineID == id))
                return NotFound("The requested machine is not scheduled for deletion");

            var deletionRequest = await _context.MachineDeletionRequests.FirstAsync(request => request.MachineID == id);
            _context.MachineDeletionRequests.Remove(deletionRequest);
            await _context.SaveChangesAsync();
            return Ok($"Machine({id}) no longer scheduled for deletion");
        }

        /// <summary>
        /// Resets a machine. Im practice this means that the machine is deleted immediately, and a new machine is created according to the same specifications as the original machine.
        /// Resetting the machine can take a fair bit of time depending upon how busy the system is.
        /// </summary>
        /// <param name="id">The id of the machine being reset</param>
        /// <returns>
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>Status Codes</term>
        ///     </listheader>
        ///     <item>
        ///         <term>200 -</term>
        ///         <description> Machine reset initialized</description>
        ///     </item>
        ///     <item>
        ///         <term>400 -</term>
        ///         <description>
        ///             1. The id was 0 or empty, and as such invalid
        ///             2. The machine was not in the configured state
        ///         </description> 
        ///     </item>
        ///     <item>
        ///         <term>401 -</term>
        ///         <description> The requesting user does not have the right to reset this machine</description>
        ///     </item>
        ///     <item>
        ///         <term>404 -</term>
        ///         <description> The id did not correspond to an existing machine</description>
        ///     </item>
        ///     <item>
        ///         <term>500 -</term>
        ///         <description> There was an error while trying to terminate the original machine</description>
        ///     </item>
        /// </list>
        /// </returns>
        [Authorize(Policy = "EducatorLevel")]
        [HttpPost]
        [Route("control/reset/{id}")]
        public async Task<ActionResult> PostResetMachine(Guid id)
        {
            //Validate id
            if (id == Guid.Empty) return BadRequest("Invalid ID");
            //Validate machine existence
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound("Machine Not Found");
            //Validate machine "ownership"
            var username = this.GetUsername();
            if (machine.UserUsername != username || !await _context.Users.Where(u => u.Username == username
                    && u.AccountType == Models.User.UserType.Administrator).AnyAsync())
                return Unauthorized("You do not own this machine");
            //Validate machine is created and configured
            if (machine.MachineCreationStatus is not CreationStatus.CONFIGURED)
                return BadRequest("Cannot reset a machine that is not created and configured");

            Log.Verbose("MachineController-ResetMachine-AttemptingMachineReset-{id}: User {user}", id,
                this.GetUsername());
            //Delete existing machine
            if (!_accessor.PerformVirtualMachineAction(MachineActions.TERMINATE_HARD,
                    (int) machine.OpenNebulaID))
            {
                Log.Error(
                    "Error while deleting machine before recreation machine_id:{machineid} OpenNebula_id:{opennebula_id} ",
                    machine.MachineID.ToString(), machine.OpenNebulaID.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "There was an error while trying to delete the machine. Contact a Virtualresources administrator for help.");
            }

            //Remove the status for the reset machine
            var status = await _context.MachineStatuses.FindAsync(machine.MachineID);
            _context.MachineStatuses.Remove(status);

            Log.Verbose("MachineController-ResetMachine-{id}: Machine Deleted", id);
            //Schedule machine for creation            
            machine.MachineCreationStatus = CreationStatus.REGISTERED;
            Log.Verbose("MachineController-ResetMachine-{id}: Machine registered for creation", id);
            _context.Machines.Update(machine);

            await _context.SaveChangesAsync();
            return Ok($"Machine({id}) has been reset");
        }

        // POST: api/machine/create/groupbased
        [Authorize(Policy = "EducatorLevel")]
        [HttpPost("create/groupbased")]
        public async Task<ActionResult<Machine>> PostMachineGroupBased(CreateMachineGroupBased machines)
        {
            var randomizer = new Random();
            //Validate input
            foreach (var machine in machines.Machines)
            {
                //Validate ownership
                var group = await _context.Groups.FindAsync(machine.Group);
                if (group == null) return BadRequest($"Invalid Group id");
                if (group.CourseID != machine.CourseID)
                    return BadRequest($"GroupID: {machine.Group} is not associated with the course {machine.CourseID}");
                if (group.Course.UserUsername != this.GetUsername())
                    return Unauthorized(
                        $"You do not have ownership over the course: {machine.CourseID}, and therefor not over the group id requested");

                //Validate Content
                if (!Regex.IsMatch(machine.Hostname, ValidateHostname))
                    return BadRequest($"Invalid Hostname: {machine.Hostname}");
                if (!machine.Apt.AsParallel().All(apt => Regex.IsMatch(apt, ValidateAptRegex)))
                    return BadRequest($"Invalid apt package in list: {string.Join(", ", machine.Apt.ToArray())}");
                if (!machine.Ppa.AsParallel().All(ppa => Regex.IsMatch(ppa, ValidatePpaRegex)))
                    return BadRequest($"Invalid ppa in list: {string.Join(", ", machine.Ppa.ToArray())}");
                if (!machine.LinuxGroups.AsParallel().All(linuxGroup => Regex.IsMatch(linuxGroup, ValidateLinuxGroup)))
                    return BadRequest($"Invalid linux group in list: {string.Join(", ", machine.LinuxGroups)}");
                if (!machine.Ports.AsParallel().All(port => port is > 0 and <= 65535))
                    return BadRequest($"Port Out of bound in list: {string.Join(", ", machine.Ports)}");
                var newMachineId = Guid.NewGuid();
                _context.Machines.Add(new Machine
                {
                    CourseID = machine.CourseID,
                    HostName = machine.Hostname,
                    MachineID = newMachineId,
                    UserUsername = this.GetUsername(),
                    MachineCreationStatus = CreationStatus.REGISTERED,
                    Apt = machine.Apt,
                    LinuxGroups = machine.LinuxGroups,
                    Ports = machine.Ports,
                    Ppa = machine.Ppa
                });
                _context.MachineAssignments.Add(new MachineAssignment
                {
                    MachineAssignmentID = Guid.NewGuid(),
                    GroupID = machine.Group,
                    MachineID = newMachineId,
                    OneTimePassword = new string(Enumerable.Repeat(Chars, 12).Select(s => s[randomizer.Next(s.Length)])
                        .ToArray()),
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
            var randomizer = new Random();
            //Validate input
            foreach (var machine in machines.Machines)
            {
                //Validate ownership
                var course = await _context.Courses.FindAsync(machine.CourseID);
                if (course == null) return BadRequest($"The requested course id:({machine.CourseID}) does not exists");
                if (course.UserUsername != this.GetUsername())
                    return Unauthorized($"You do not own the course({course.CourseID})");

                //Validate Content
                if (!Regex.IsMatch(machine.Hostname, ValidateHostname))
                    return BadRequest($"Invalid Hostname: {machine.Hostname}");
                if (!machine.Apt.AsParallel().All(apt => Regex.IsMatch(apt, ValidateAptRegex)))
                    return BadRequest($"Invalid apt package in list: {string.Join(", ", machine.Apt.ToArray())}");
                if (!machine.Ppa.AsParallel().All(ppa => Regex.IsMatch(ppa, ValidatePpaRegex)))
                    return BadRequest($"Invalid ppa in list: {string.Join(", ", machine.Ppa.ToArray())}");
                if (!machine.LinuxGroups.AsParallel().All(group => Regex.IsMatch(group, ValidateLinuxGroup)))
                    return BadRequest($"Invalid linux group in list: {string.Join(", ", machine.LinuxGroups)}");
                if (!machine.Ports.AsParallel().All(port => port is > 0 and <= 65535))
                    return BadRequest($"Port Out of bound in list: {string.Join(", ", machine.Ports)}");
                var newMachineId = Guid.NewGuid();
                _context.Machines.Add(new Machine
                {
                    CourseID = machine.CourseID,
                    HostName = machine.Hostname,
                    MachineID = newMachineId,
                    UserUsername = this.GetUsername(),
                    MachineCreationStatus = CreationStatus.REGISTERED
                });
                _context.MachineAssignments.Add(new MachineAssignment
                {
                    MachineAssignmentID = Guid.NewGuid(),
                    GroupID = null,
                    MachineID = newMachineId,
                    OneTimePassword = new string(Enumerable.Repeat(Chars, 12).Select(s => s[randomizer.Next(s.Length)])
                        .ToArray()),
                    UserUsername = null
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Machines Registered for creation, creation will start soon.");
        }
    }
}