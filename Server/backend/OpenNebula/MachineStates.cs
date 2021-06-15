using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.OpenNebula
{
    public enum MachineStates
    {
        /// <summary>
        /// Internal initialization state right after VM creation, this state is not visible for the end users.
        /// </summary>
        INIT,
        /// <summary>
        /// Virtual machines start in the pending state while waiting for the scheduler to deploy them. 
        /// </summary>
        PENDING,
        /// <summary>
        /// The virtual machine has been manualy put into a hold state.
        /// </summary>
        HOLD,
        /// <summary>
        /// The machine is active, and any more detail about the state
        /// can be found in the <see cref="LCMStates"/>.
        /// </summary>
        ACTIVE,
        /// <summary>
        /// The virtual machine is stopped, with the vm state saved and the image and state have been transferred
        /// to the system datastore.
        /// </summary>
        STOPPED,
        /// <summary>
        /// The same as <see cref="STOPPED"/>, but the files are left on the host machine for a quicker resume.
        /// </summary>
        SUSPENDED,
        /// <summary>
        /// The virtual machine is done. Once in this state the machine is deleted,
        /// but information about the machine can still be retrieved
        /// </summary>
        DONE,
        /// <summary>
        /// This state exists for compatability, but is otherwise undocumented on <see href="https://docs.opennebula.io/5.8/operation/references/vm_states.html#vm-states">docs.opennebula.io</see>
        /// </summary>
        FAILED,
        /// <summary>
        /// The virtual machine is powered down, and the files are still on the host machine for future power up.
        /// </summary>
        POWEROFF,
        /// <summary>
        /// The VM is shut down. Similar to <see cref="STOPPED"/>, but no checkpoint file is generated.
        /// The VM disks are transfered to the system datastore. The VM can be resumed later.
        /// </summary>
        UNDEPLOYED,
        /// <summary>
        /// The virtual machine is waiting for one or more disks to finish copying.
        /// </summary>
        CLONING,
        /// <summary>
        /// Failure during cloning.
        /// </summary>
        CLONING_FALIURE
    }
}
