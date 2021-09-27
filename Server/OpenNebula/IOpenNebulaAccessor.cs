using ScalableTeaching.OpenNebula.Models;
using System.Collections.Generic;

namespace ScalableTeaching.OpenNebula
{
    public interface IOpenNebulaAccessor
    {
        /// <summary>
        /// Retrieves the information for the specific virutal machine
        /// </summary>
        /// <param name="VirtualMachineId">Id of the desired virtual machine</param>
        /// <returns></returns>
        public VmModel GetVirtualMachineInformation(int VirtualMachineId);
        /// <summary>
        /// Gets information about virutal machines in the virtual machine pool
        /// </summary>
        /// <param name="IncludeDoneMachines">True if deleted machines are desired</param>
        /// <param name="OwnershipMask"><list type="table">
        /// <listheader>Owner Filtering</listheader>
        /// <item><term>-4</term><description> Resources belonging to the user’s primary group.</description></item>
        /// <item><term>-3</term><description> Resources belonging to the user.</description></item>
        /// <item><term>-2</term><description> All resources.</description></item>
        /// <item><term>-1</term><description> Resources belonging to the user and any of his groups.</description></item>
        /// </list></param>
        /// <returns></returns>
        public List<VmModel> GetAllVirtualMachineInfo(bool IncludeDoneMachines, int OwnershipMask);

        /// <summary>
        /// Performs an action upon the virual machine.
        /// Valid actions are defined in <see cref="MachineActions"/>
        /// </summary>
        /// <param name="Session">Username:Password</param>
        /// <param name="Action">An action string from <see cref="MachineActions"/></param>
        /// <param name="VirtualMachineId">Integer id matching the id of an existing machine</param>
        /// <returns>success</returns>
        public bool PerformVirtualMachineAction(string Action, int VirtualMachineId);
        /// <summary>
        /// Retrives the information on the available templates
        /// </summary>
        /// <returns></returns>
        public List<VmTemplateModel> GetAllVirtualMachineTemplateInfo();

        /// <summary>
        /// Instantiates a virtual machine
        /// </summary>
        /// <param name="TemplateId">Id of template for the virtual machine </param>
        /// <param name="VirtualMachineName">The future name of the virutal machine</param>
        /// <param name="memmory">Specifies the amount of ram for the machine to be allocated</param>
        /// <param name="vcpu">Specifies the number of vcpu that the machine is allocated</param>
        /// <returns>Touple of success and new machine id</returns>
        public (bool, int) CreateVirtualMachine(int TemplateId, string VirtualMachineName, int memmory = 1024, int vcpu = 1);

    }
}
