using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScalableTeaching.Data;
using ScalableTeaching.DTO;
using ScalableTeaching.Helpers;
using ScalableTeaching.Models;
using System.Text.RegularExpressions;
using static ScalableTeaching.Controllers.Extensions.HttpContextExtensions;

namespace ScalableTeaching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "EducatorLevel")]
    public class CreationController : ControllerBase
    {
        private string ValidateAptRegex = @"^[0-9A-Za-z.+-]+$";
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
            foreach (var machine in machines.Machines)
            {
                //Validate ownership
                var group = await _context.Groups.FindAsync(machine.Group);
                if (group == null) return BadRequest($"Invalid Group id");
                if (group.CourseID != machine.CourseID) return BadRequest($"GroupID: {machine.Group} is not assiciated with the course {machine.CourseID}");
                if (group.Course.UserUsername != this.GetUsername()) return Unauthorized($"You do not have ownership over the course: {machine.CourseID}, and therefor not over the group id requested");

                //Validate Content
                if (!Regex.IsMatch(machine.Hostname, ValidateHostname)) return BadRequest($"Invalid Hostname: {machine.Hostname}");
                if (!machine.Apt.AsParallel().All(apt => Regex.IsMatch(apt, ValidateAptRegex))) return BadRequest($"Invalid apt package in list: {String.Join(", ", machine.Apt.ToArray())}");
                if (!machine.Ppa.AsParallel().All(ppa => Regex.IsMatch(ppa, ValidatePpaRegex))) return BadRequest($"Invalid ppa in list: {String.Join(", ", machine.Ppa.ToArray())}");
                if (!machine.LinuxGroups.AsParallel().All(group => Regex.IsMatch(group, ValidateLinuxGroup))) return BadRequest($"Invalid linux group in list: {String.Join(", ", machine.LinuxGroups)}");
                if (!machine.Ports.AsParallel().All(port => port > 0 && port <= 65535)) return BadRequest($"Port Out of bound in list: {String.Join(", ", machine.Ports)}");

                if (machine.VCPU != null && (machine.VCPU < 1 || machine.VCPU > 8)) return BadRequest($"Machine VCPU count out of bounds: {machine.VCPU}, VCPU count must be greater than 0 and less than 9");

                if (machine.Memory != null && (machine.Memory < 1024 || machine.Memory > 8192)) return BadRequest($"Machine memory amount out of bounds: {machine.Storage}, Memory must be greater that 1023 and less than 8193");
                if (machine.Memory != null && machine.Memory % 1024 != 0) return BadRequest($"Invalid memory amount: {machine.Storage}, Memory must be a multiple of 1024");

                if (machine.Storage != null && (machine.Storage < 30720 || machine.Storage > 51200)) return BadRequest($"Machine storage amount out of bounds: {machine.Storage}, Memmory must be greater that 30719 and less than 51201");
                if (machine.Storage != null && machine.Storage % 1024 != 0) return BadRequest($"Invalid memmory amount: {machine.Storage}, Memmory must be a multiple of 1024");

                var NewMachineID = Guid.NewGuid();
                _context.Machines.Add(new Machine
                {
                    CourseID = machine.CourseID,
                    HostName = machine.Hostname,
                    MachineID = NewMachineID,
                    UserUsername = this.GetUsername(),
                    MachineCreationStatus = CreationStatus.REGISTERED,
                    Apt = machine.Apt,
                    LinuxGroups = machine.LinuxGroups,
                    Ports = machine.Ports,
                    Ppa = machine.Ppa,
                    Memory = machine.Memory ?? 1024,
                    VCPU = machine.VCPU ?? 1,
                    Storage = machine.Storage ?? 30720
                });
                _context.MachineAssignments.Add(new()
                {
                    MachineAssignmentID = Guid.NewGuid(),
                    GroupID = machine.Group,
                    MachineID = NewMachineID,
                    OneTimePassword = new string(Enumerable.Repeat(chars, 12).Select(s => s[Randomizer.Next(s.Length)]).ToArray()),
                    UserUsername = null
                }); ;
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
                if (course.UserUsername != this.GetUsername()) return Unauthorized($"You do not own the course({course.CourseID})");

                //Validate Content
                if (!Regex.IsMatch(machine.Hostname, ValidateHostname)) return BadRequest($"Invalid Hostname: {machine.Hostname}");
                if (!machine.Apt.AsParallel().All(apt => Regex.IsMatch(apt, ValidateAptRegex))) return BadRequest($"Invalid apt package in list: {String.Join(", ", machine.Apt.ToArray())}");
                if (!machine.Ppa.AsParallel().All(ppa => Regex.IsMatch(ppa, ValidatePpaRegex))) return BadRequest($"Invalid ppa in list: {String.Join(", ", machine.Ppa.ToArray())}");
                if (!machine.LinuxGroups.AsParallel().All(group => Regex.IsMatch(group, ValidateLinuxGroup))) return BadRequest($"Invalid linux group in list: {String.Join(", ", machine.LinuxGroups)}");
                if (!machine.Ports.AsParallel().All(port => port > 0 && port <= 65535)) return BadRequest($"Port Out of bound in list: {String.Join(", ", machine.Ports)}");
                
                if (machine.VCPU != null && (machine.VCPU < 1 || machine.VCPU > 8)) return BadRequest($"Machine VCPU count out of bounds: {machine.VCPU}, VCPU count must be greater than 0 and less than 9");
                
                if (machine.Memory != null && (machine.Memory < 1024 || machine.Memory > 8192)) return BadRequest($"Machine memmory amount out of bounds: {machine.Memory}, Memmory must be greater that 1023 and less than 8193");
                if (machine.Memory != null && machine.Memory % 1024 != 0) return BadRequest($"Invalid memmory amount: {machine.Memory}, Memmory must be a multiple of 1024");

                if (machine.Storage != null && (machine.Storage < 30720 || machine.Storage > 51200)) return BadRequest($"Machine storage amount out of bounds: {machine.Storage}, Memmory must be greater that 30719 and less than 51201");
                if (machine.Storage != null && machine.Storage % 1024 != 0) return BadRequest($"Invalid memmory amount: {machine.Storage}, Memmory must be a multiple of 1024");


                var NewMachineID = Guid.NewGuid();
                _context.Machines.Add(new Machine
                {
                    CourseID = machine.CourseID,
                    HostName = machine.Hostname,
                    MachineID = NewMachineID,
                    UserUsername = this.GetUsername(),
                    MachineCreationStatus = CreationStatus.REGISTERED,
                    Apt = machine.Apt,
                    LinuxGroups = machine.LinuxGroups,
                    Ports = machine.Ports,
                    Ppa = machine.Ppa,
                    Memory = machine.Memory ?? 1024,
                    VCPU = machine.VCPU ?? 1,
                    Storage = machine.Storage ?? 30720
                });
                foreach (var user in machine.Users)
                {
                    if (_context.Users.Find(user.ToLower()) == null)
                        _context.Users.Add(await UserFactory.Create(user));
                    _context.MachineAssignments.Add(new()
                    {
                        MachineAssignmentID = Guid.NewGuid(),
                        GroupID = null,
                        MachineID = NewMachineID,
                        OneTimePassword = new string(Enumerable.Repeat(chars, 12).Select(s => s[Randomizer.Next(s.Length)]).ToArray()),
                        UserUsername = user
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Machines Registered for creation, creation will start soon.");
        }
    }
}
