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
        public DateTimeOffset LastPoll { get; }
        public MachineStates MachineState { get; }
        public LCMStates MachineLCMState { get; }
        public decimal MachineCpuUtilizationPercent { get; }
        public int MachineMemmoryUtilizationBytes { get; }
        public string MachineIp { get; }
        public string MachineMac { get; }

        public virtual Machine Machine { get; set; }


        public MachineStatus() { }
        public MachineStatus(Guid MachineID, VmModel MachineStatus) {
            this.MachineID = MachineID;
            this.MachineCpuUtilizationPercent = MachineStatus.MachineCpuUtilizationPercent;
            this.MachineIp = MachineStatus.MachineIp;
            this.MachineLCMState = MachineStatus.MachineLCMState;
            this.MachineMac = MachineStatus.MachineMac;
            this.MachineMemmoryUtilizationBytes = MachineStatus.MachineMemmoryUtilizationBytes;
            this.MachineState = MachineStatus.MachineState;
        }
    }
}
