using System.Collections.Generic;
using ScalableTeaching.OpenNebula.Models;

namespace ScalableTeaching.OpenNebula
{
    interface IOpenNebulaAccessor
    {
        public VmModel GetVirtualMachineInformation(int VirtualMachineId);

        public List<VmModel> GetAllVirtualMachineInfo(bool IncludeDoneMachines);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Session">Username:Password</param>
        /// <param name="Action">An action string from <see cref="MachineActions"/></param>
        /// <param name="VirtualMachineId">Integer id matching the id of an existing machine</param>
        /// <returns></returns>
        public bool PerformVirtualMachineAction(string Action, int VirtualMachineId);
        public List<VmTemplateModel> GetAllVirtualMachineTemplateInfo();
        public (bool, int) CreateVirtualMachine(int TemplateId, string VirtualMachineName);

    }
}
