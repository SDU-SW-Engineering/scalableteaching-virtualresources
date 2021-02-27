using backend.Data;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<string> GetSingleMachineCredentialStringAsync(MachineCredentail credential)
        {
            Machine machine = await _context.Machines.FindAsync(credential.MachineID);
            List<Port> ports = _context.Ports.Where(port => port.MachineID == credential.MachineID).ToList();
            return null;
        }
    }
}
