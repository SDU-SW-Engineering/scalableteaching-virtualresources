using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.OpenNebula
{
    public static class MachineActions
    {
        /// <summary>
        /// Instantly deletes the virtual machine.
        /// </summary>
        public static readonly string TERMINATE_HARD = "terminate-hard";
        /// <summary>
        /// Shuts down the virtual machine using an ACPI signal,
        /// followed by the deletion of the machine.
        /// </summary>
        public static readonly string TERMINATE = "terminate";
        /// <summary>
        /// Same as <see cref="UNDEPLOY"/>
        /// but the machine is immediatly destroyed.
        /// </summary>
        public static readonly string UNDEPLOY_HARD = "undeploy-hard";
        /// <summary>
        /// A way to long term pause a virtual machine.
        /// The Host resources used by the vm are freed. Any disk is saved in 
        /// the system datastore.
        /// <para>The machine is shut down,
        /// followed by the freeing of the resources.</para>
        /// </summary>
        public static readonly string UNDEPLOY = "undeploy";
        /// <summary>
        /// Instantly poweroff the virtual machine: <b>WARNIING</b>, 
        /// this is not a gracefull shutdown
        /// </summary>
        public static readonly string POWEROFF_HARD = "poweroff-hard";
        /// <summary>
        /// Poweroff the virtualmachine gracefully, using an ACPI 
        /// </summary>
        public static readonly string POWEROFF = "poweroff";
        /// <summary>
        /// Performs a hard reboot i.e., the machine is basically power cycled.
        /// </summary>
        public static readonly string REBOOT_HARD = "reboot-hard";
        /// <summary>
        /// Starts a gracefull reboot using an ACPI signal.
        /// </summary>
        public static readonly string REBOOT = "reboot";
        /// <summary>
        /// Puts a machine that is in <see cref="MachineStates.PENDING"/> state
        /// into the <see cref="MachineStates.HOLD"/> state, stopping the 
        /// scheduler from deploying the machine.
        /// </summary>
        public static readonly string HOLD = "hold";
        /// <summary>
        /// Returning the virtual machine to the 
        /// <see cref="MachineStates.PENDING"/> state, allowing the scheduler to deploy the machine
        /// </summary>
        public static readonly string RELEASE = "release";
        /// <summary>
        /// Same as <see cref="UNDEPLOY"/>, but instead of shutting down,
        /// the state is saved.
        /// </summary>
        public static readonly string STOP = "stop";
        /// <summary>
        /// Saves the state of the virtual machine.
        /// </summary>
        public static readonly string SUSPEND = "suspend";
        /// <summary>
        /// Resumes the execution of VMs in the <see cref="MachineStates.STOPPED"/>, <see cref="MachineStates.SUSPENDED"/>, <see cref="MachineStates.UNDEPLOY"/> and <see cref="MachineStates.POWEROFF"/> states.
        /// </summary>
        public static readonly string RESUME = "resume";

        private static List<string> _ListOfActions = new() { TERMINATE_HARD, TERMINATE, UNDEPLOY_HARD, UNDEPLOY, POWEROFF_HARD, POWEROFF, REBOOT_HARD, REBOOT, HOLD, RELEASE, STOP, SUSPEND, RESUME };
        /// <summary>
        /// List of actions that can be performed on virtual machines
        /// </summary>
        public static IReadOnlyList<string> ListOfActions { get { return _ListOfActions.AsReadOnly(); } }
    }
}
