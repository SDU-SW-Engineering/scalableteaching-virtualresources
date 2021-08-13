using System;
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
        public virtual User User { get; set; }
        public virtual Course Course { get; set; }

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
        DELETED
    }

    
}
