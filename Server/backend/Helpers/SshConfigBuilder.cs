using backend.Data;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Helpers
{
    public class SshConfigBuilder
    {
        private readonly VmDeploymentContext _context;

        public SshConfigBuilder(VmDeploymentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Generates a configuration string for an ssh config
        /// </summary>
        /// <param name="assignment">The credential object representing a credential pair for a user</param>
        /// <param name="includeClassName">True indicates that the host name will be prefixed with the short version of the course name</param>
        /// <returns>Config string for specified machine</returns>
        public async Task<string> GetMachineCredentialStringAsync(MachineAssignment assignment, bool includeClassName = false)
        {
            Machine machine = await _context.Machines.FindAsync(assignment.MachineID);
            var machineName = machine.Name;
            var hostName = machine.HostName;
            var username = machine.User.Username;
            var course = machine.Course;
            List<LocalForward> LocalForwards = _context.LocalForwards.Where(port => port.MachineID == assignment.MachineID).ToList();

            StringBuilder credentialBuilder = new StringBuilder();

            //Insert host name string
            credentialBuilder.Append("Host ");
            if (includeClassName)
            {
                credentialBuilder.Append(course.ShortCourseName);
            }
            credentialBuilder.Append(machineName).Append('\n');

            //Insert HostName url string
            credentialBuilder.Append('\t').Append("HostName ").Append(hostName).Append('\n');

            //Insert Username for the machine
            credentialBuilder.Append('\t').Append("User").Append(username).Append('\n');

            //Insert strings for portnumbers
            foreach (var port in LocalForwards)
            {
                credentialBuilder.Append('\t').Append("LocalForward ").Append(port.PortNumber).Append("127.0.0.1:").Append(port.PortNumber).Append('\n');
            }

            //Insert Identityfile string
            credentialBuilder.Append('\t').Append("IdentityFile ").Append("~/.ssh/id_rsa_scalable_").Append(assignment.UserUsername);

            return credentialBuilder.ToString();
        }

        public void BuildAuthorisedKeysFile(Machine machine)
        {
            var assignments = _context.MachineAssignments.Where(assignment =>
                assignment.MachineID == machine.MachineID);
            //TODO : Continue this
        }
    }
}
