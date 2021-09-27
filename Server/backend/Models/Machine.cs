using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScalableTeaching.Models
{
    public class Machine
    {
        [Key]
        public Guid MachineID { get; set; }
        [Required]
        public string HostName { get; set; }
        [Required]
        public string UserUsername { get; set; }
        [Required]
        public Guid CourseID { get; set; }
        public CreationStatus MachineCreationStatus { get; set; }
        public int? OpenNebulaID { get; set; }
        public List<string> Apt { get; set; }
        public List<string> Ppa { get; set; }
        public List<string> LinuxGroups { get; set; }
        public List<int> Ports { get; set; }
        public int Memmory { get; set; }
        public int VCPU { get; set; }

        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
        public virtual MachineStatus MachineStatus { get; set; }
        public virtual List<MachineAssignment> MachineAssignments { get; set; }

        public override string ToString()
        {
            return $"MachineID: {MachineID}, HostName: {HostName}, UserUsername: {UserUsername}, CourseID: {CourseID}, MachineCreationStatus: {MachineCreationStatus}, OpenNebulaID: {OpenNebulaID}";
        }
    }
    public enum CreationStatus
    {
        REGISTERED,
        QUEUED_FOR_CREATION,
        CREATED,
        QUEUED_FOR_CONFIGURATION,
        CONFIGURED,
        SHEDULED_FOR_DELETION,
        DELETED
    }
}
