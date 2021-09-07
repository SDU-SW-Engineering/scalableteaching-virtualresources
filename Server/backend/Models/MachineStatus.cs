using ScalableTeaching.OpenNebula;
using ScalableTeaching.OpenNebula.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
        public MachineStatus()
        {

        }
    }
}
