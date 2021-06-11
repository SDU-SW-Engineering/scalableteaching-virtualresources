using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.XmlRpc
{
    public struct VmModel
    {
        //TODO: Possibly implement DiskUtilization if parameters available
        public int MachineId { get; }
        public string MachineName { get; }
        public DateTimeOffset LastPoll { get; }
        public MachineState MachineState { get; }
        public decimal MachineCpuUtilizationPercent { get; }
        public int MachineMemmoryUtilizationBytes { get; }
        public string MachineIp { get; }
        public string MachineMac { get; }

        public VmModel(
            int machineId,
            string machineName,
            DateTimeOffset lastPoll,
            MachineState machineState,
            decimal machineCpuUtilizationPercent,
            int machineMemmoryUtilizationBytes,
            string machineIp,
            string machineMac)
        {
            MachineId = machineId;
            MachineName = machineName;
            LastPoll = lastPoll;
            MachineState = machineState;
            MachineCpuUtilizationPercent = machineCpuUtilizationPercent;
            MachineMemmoryUtilizationBytes = machineMemmoryUtilizationBytes;
            MachineIp = machineIp;
            MachineMac = machineMac;
        }
    }
}
