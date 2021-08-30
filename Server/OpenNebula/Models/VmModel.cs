using System;

namespace ScalableTeaching.OpenNebula.Models
{
    public struct VmModel
    {
        //TODO: Possibly implement DiskUtilization if parameters available
        public readonly int MachineId { get; }
        public readonly string MachineName { get; }
        public readonly DateTimeOffset LastPoll { get; }
        public readonly MachineStates MachineState { get; }
        public readonly LCMStates MachineLCMState { get; }
        public readonly decimal MachineCpuUtilizationPercent { get; }
        public readonly int MachineMemmoryUtilizationBytes { get; }
        public readonly string MachineIp { get; }
        public readonly string MachineMac { get; }

        public VmModel(
            int machineId,
            string machineName,
            DateTimeOffset lastPoll,
            MachineStates machineState,
            LCMStates LCMState,
            decimal machineCpuUtilizationPercent,
            int machineMemmoryUtilizationBytes,
            string machineIp,
            string machineMac)
        {
            MachineId = machineId;
            MachineName = machineName;
            LastPoll = lastPoll;
            MachineState = machineState;
            MachineLCMState = LCMState;
            MachineCpuUtilizationPercent = machineCpuUtilizationPercent;
            MachineMemmoryUtilizationBytes = machineMemmoryUtilizationBytes;
            MachineIp = machineIp;
            MachineMac = machineMac;
        }
    }
}
