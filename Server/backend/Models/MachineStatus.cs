using ScalableTeaching.OpenNebula;
using ScalableTeaching.OpenNebula.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class MachineStatus
    {
        [Key]
        public Guid MachineID { get; set; }
        public DateTimeOffset LastPoll { get; set; }
        public MachineStates MachineState { get; set; }
        public LCMStates MachineLCMState { get; set; }
        public decimal MachineCpuUtilizationPercent { get; set; }
        public int MachineMemmoryUtilizationBytes { get; set; }
        public string MachineIp { get; set; }
        public string MachineMac { get; set; }

        public virtual Machine Machine { get; set; }

        public static MachineStatus MachineStatusFactory(Guid MachineID, VmModel MachineStatus, DateTimeOffset PollTime)
        {
            return new MachineStatus()
            {
                MachineID = MachineID,
                MachineCpuUtilizationPercent = MachineStatus.MachineCpuUtilizationPercent,
                MachineIp = MachineStatus.MachineIp,
                MachineLCMState = MachineStatus.MachineLCMState,
                MachineMac = MachineStatus.MachineMac,
                MachineMemmoryUtilizationBytes = MachineStatus.MachineMemmoryUtilizationBytes,
                MachineState = MachineStatus.MachineState,
                LastPoll = PollTime.ToUniversalTime(),
            };
        }

        public MachineStatus Update(MachineStatus NewStatus)
        {
            MachineCpuUtilizationPercent = NewStatus.MachineCpuUtilizationPercent;
            MachineIp = NewStatus.MachineIp;
            MachineLCMState = NewStatus.MachineLCMState;
            MachineMac = NewStatus.MachineMac;
            MachineMemmoryUtilizationBytes = NewStatus.MachineMemmoryUtilizationBytes;
            MachineState = NewStatus.MachineState;
            LastPoll = NewStatus.LastPoll;
            return this;
        }

        public MachineStatus()
        {

        }
    }
}
