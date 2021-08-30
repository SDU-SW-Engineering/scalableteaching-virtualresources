namespace ScalableTeaching.OpenNebula
{
    public enum LCMStates
    {
        /// <summary>
        /// Internal initialization state, not visible for the end users
        /// </summary>
        LCM_INIT,
        /// <summary>
        /// The system is transferring the VM files (disk images and the recovery file) to the host in which the virtual machine will be running.
        /// </summary>
        PROLOG,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM
        /// </summary>
        BOOT,
        /// <summary>
        /// The VM is running (note that this stage includes the internal virtualized machine booting and shutting down phases). In this state, the virtualization driver will periodically monitor it
        /// </summary>
        RUNNING,
        /// <summary>
        /// The VM is migrating from one host to another. This is a hot migration
        /// </summary>
        MIGRATE,
        /// <summary>
        /// The system is saving the VM files after a stop operation
        /// </summary>
        SAVE_STOP,
        /// <summary>
        /// The system is saving the VM files after a suspend operation
        /// </summary>
        SAVE_SUSPEND,
        /// <summary>
        /// The system is saving the VM files for a cold migration
        /// </summary>
        SAVE_MIGRATE,
        /// <summary>
        /// File transfers during a cold migration
        /// </summary>
        PROLOG_MIGRATE,
        /// <summary>
        /// File transfers after a resume action (from stopped)
        /// </summary>
        PROLOG_RESUME,
        /// <summary>
        /// File transfers from the Host to the system datastore
        /// </summary>
        EPILOG_STOP,
        /// <summary>
        /// The system cleans up the Host used to virtualize the VM, and additionally disk images to be saved are copied back to their datastores
        /// </summary>
        EPILOG,
        /// <summary>
        /// OpenNebula has sent the VM the shutdown ACPI signal, and is waiting for it to complete the shutdown process. If after a timeout period the VM does not disappear, OpenNebula will assume that the guest OS ignored the ACPI signal and the VM state will be changed to running, instead of done
        /// </summary>
        SHUTDOWN,
        /// <summary>
        /// Cleanup after a delete-recreate action
        /// </summary>
        CLEANUP_RESUBMIT = 15,
        /// <summary>
        /// The VM couldn’t be monitored, it is in an unknown state
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// A disk attach/detach operation is in progress
        /// </summary>
        HOTPLUG,
        /// <summary>
        /// OpenNebula has sent the VM the shutdown ACPI signal, and is waiting for it to complete the shutdown process. If after a timeout period the VM does not disappear, OpenNebula will assume that the guest OS ignored the ACPI signal and the VM state will be changed to running, instead of poweroff
        /// </summary>
        SHUTDOWN_POWEROFF,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM (from UNKNOWN)
        /// </summary>
        BOOT_UNKNOWN,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM (from POWEROFF)
        /// </summary>
        BOOT_POWEROFF,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM (from SUSPENDED)
        /// </summary>
        BOOT_SUSPENDED,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM (from STOPPED)
        /// </summary>
        BOOT_STOPPED,
        /// <summary>
        /// Cleanup after a delete action
        /// </summary>
        CLEANUP_DELETE = 23,
        /// <summary>
        /// A system snapshot action is in progress
        /// </summary>
        HOTPLUG_SNAPSHOT,
        /// <summary>
        /// A NIC attach/detach operation is in progress
        /// </summary>
        HOTPLUG_NIC,
        /// <summary>
        /// A disk-saveas operation is in progress
        /// </summary>
        HOTPLUG_SAVEAS,
        /// <summary>
        /// A disk-saveas operation (from POWEROFF) is in progress
        /// </summary>
        HOTPLUG_SAVEAS_POWEROFF,
        /// <summary>
        /// A disk-saveas operation (from SUSPENDED) is in progress
        /// </summary>
        HOTPLUG_SAVEAS_SUSPENDED,
        /// <summary>
        /// OpenNebula has sent the VM the shutdown ACPI signal, and is waiting for it to complete the shutdown process. If after a timeout period the VM does not disappear, OpenNebula will assume that the guest OS ignored the ACPI signal and the VM state will be changed to running, instead of undeployed
        /// </summary>
        SHUTDOWN_UNDEPLOY,
        /// <summary>
        /// The system cleans up the Host used to virtualize the VM, and VM files are transfered to the system datastore
        /// </summary>
        EPILOG_UNDEPLOY,
        /// <summary>
        /// File transfers after a resume action (from undeployed)
        /// </summary>
        PROLOG_UNDEPLOY,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM (from UNDEPLOY)
        /// </summary>
        BOOT_UNDEPLOY,
        /// <summary>
        /// File transfers for a disk attach from poweroff
        /// </summary>
        HOTPLUG_PROLOG_POWEROFF,
        /// <summary>
        /// File transfers for a disk detach from poweroff
        /// </summary>
        HOTPLUG_EPILOG_POWEROFF,
        /// <summary>
        /// OpenNebula is waiting for the hypervisor to create the VM (from a cold migration)
        /// </summary>
        BOOT_MIGRATE,
        /// <summary>
        /// Failure during a BOOT
        /// </summary>
        BOOT_FAILURE,
        /// <summary>
        /// Failure during a BOOT_MIGRATE
        /// </summary>
        BOOT_MIGRATE_FAILURE,
        /// <summary>
        /// Failure during a PROLOG_MIGRATE
        /// </summary>
        PROLOG_MIGRATE_FAILURE,
        /// <summary>
        /// Failure during a PROLOG
        /// </summary>
        PROLOG_FAILURE,
        /// <summary>
        /// Failure during an EPILOG
        /// </summary>
        EPILOG_FAILURE,
        /// <summary>
        /// Failure during an EPILOG_STOP
        /// </summary>
        EPILOG_STOP_FAILURE,
        /// <summary>
        /// Failure during an EPILOG_UNDEPLOY
        /// </summary>
        EPILOG_UNDEPLOY_FAILURE,
        /// <summary>
        /// File transfers during a cold migration (from POWEROFF)
        /// </summary>
        PROLOG_MIGRATE_POWEROFF,
        /// <summary>
        /// Failure during a PROLOG_MIGRATE_POWEROFF
        /// </summary>
        PROLOG_MIGRATE_POWEROFF_FAILURE,
        /// <summary>
        /// File transfers during a cold migration (from SUSPEND)
        /// </summary>
        PROLOG_MIGRATE_SUSPEND,
        /// <summary>
        /// Failure during a PROLOG_MIGRATE_SUSPEND
        /// </summary>
        PROLOG_MIGRATE_SUSPEND_FAILURE,
        /// <summary>
        /// Failure during a BOOT_UNDEPLOY
        /// </summary>
        BOOT_UNDEPLOY_FAILURE,
        /// <summary>
        /// Failure during a BOOT_STOPPED
        /// </summary>
        BOOT_STOPPED_FAILURE,
        /// <summary>
        /// Failure during a PROLOG_RESUME
        /// </summary>
        PROLOG_RESUME_FAILURE,
        /// <summary>
        /// Failure during a PROLOG_UNDEPLOY
        /// </summary>
        PROLOG_UNDEPLOY_FAILURE,
        /// <summary>
        /// A disk-snapshot-create action (from POWEROFF) is in progress
        /// </summary>
        DISK_SNAPSHOT_POWEROFF,
        /// <summary>
        /// A disk-snapshot-revert action (from POWEROFF) is in progress
        /// </summary>
        DISK_SNAPSHOT_REVERT_POWEROFF,
        /// <summary>
        /// A disk-snapshot-delete action (from POWEROFF) is in progress
        /// </summary>
        DISK_SNAPSHOT_DELETE_POWEROFF,
        /// <summary>
        /// A disk-snapshot-create action (from SUSPENDED) is in progress
        /// </summary>
        DISK_SNAPSHOT_SUSPENDED,
        /// <summary>
        /// A disk-snapshot-revert action (from SUSPENDED) is in progress
        /// </summary>
        DISK_SNAPSHOT_REVERT_SUSPENDED,
        /// <summary>
        /// A disk-snapshot-delete action (from SUSPENDED) is in progress
        /// </summary>
        DISK_SNAPSHOT_DELETE_SUSPENDED,
        /// <summary>
        /// A disk-snapshot-create action (from RUNNING) is in progress
        /// </summary>
        DISK_SNAPSHOT,
        /// <summary>
        /// A disk-snapshot-delete action (from RUNNING) is in progress
        /// </summary>
        DISK_SNAPSHOT_DELETE,
        /// <summary>
        /// File transfers during a cold migration (from UNKNOWN)
        /// </summary>
        PROLOG_MIGRATE_UNKNOWN,
        /// <summary>
        /// Failure during a PROLOG_MIGRATE_UNKNOWN
        /// </summary>
        PROLOG_MIGRATE_UNKNOWN_FAILURE,
        /// <summary>
        /// Disk resize with the vm on RUNNING state.
        /// </summary>
        DISK_RESIZE,
        /// <summary>
        /// Disk resize with the vm on POWEROFF state.
        /// </summary>
        DISK_RESIZE_POWEROFF,
        /// <summary>
        /// Disk resize with the vm UNDEPLOYED.
        /// </summary>
        DISK_RESIZE_UNDEPLOYED

    }
}