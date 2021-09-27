using CookComputing.XmlRpc;
using ScalableTeaching.OpenNebula.Containers;
using ScalableTeaching.OpenNebula.Models;
using ScalableTeaching.OpenNebula.XmlRpcInterfaces;
using System;
using System.Collections.Generic;

namespace ScalableTeaching.OpenNebula
{
    public class OpenNebulaAccessor : IOpenNebulaAccessor
    {
        private readonly IVmManagement VmManagementProxy;
        private readonly IVmTemplateManagement VmTemplateManagementProxy;
        private readonly string _Session;


        public OpenNebulaAccessor(string ServerUrl, string Session)
        {
            VmManagementProxy = XmlRpcProxyGen.Create<IVmManagement>();
            VmTemplateManagementProxy = XmlRpcProxyGen.Create<IVmTemplateManagement>();
            VmManagementProxy.Url = ServerUrl;
            VmTemplateManagementProxy.Url = ServerUrl;
            _Session = Session;
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.CreateVirtualMachine(int, string, int, int)(int, string)"/>
        public (bool, int) CreateVirtualMachine(int TemplateId, string VirtualMachineName, int memmory = 1024, int vcpu = 1)
        {
            if (!(memmory % 1024 == 0 && memmory <= 8192 && memmory >= 1024)) throw new ArgumentException("The given memmory value was invalid. Memmory must be a multiple of 1024 and at max 8192");
            if (!(vcpu >= 1 && vcpu <= 8)) throw new ArgumentException("The given cpu count was invalid. cpu count must be a natural number no greater than 8");

            string TemplateString = $"MEMMORY={memmory}\nVCPU={vcpu}";
            Object[] XmlRpcReturn = VmTemplateManagementProxy.VmTemplateInstantiate(_Session, TemplateId, VirtualMachineName, false, TemplateString);
            return ((bool)XmlRpcReturn[0], (int)XmlRpcReturn[1]);
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.GetVirtualMachineInformation(int)"/>
        public List<VmModel> GetAllVirtualMachineInfo(bool IncludeDoneMachines, int OwnershipMask)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmPoolInfoExtended(_Session, OwnershipMask, -1, -1, IncludeDoneMachines ? -2 : -1);
            VmPoolInfoExtendedReturnContainer returnContainer = new(XmlRpcReturn);
            return returnContainer.VmModelList;
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.GetAllVirtualMachineTemplateInfo()"/>
        public List<VmTemplateModel> GetAllVirtualMachineTemplateInfo()
        {
            Object[] XmlRpcReturn = VmTemplateManagementProxy.VmTemplatePoolInfo(_Session, -2, -1, -1);
            VmTemplatepoolInfoReturnContainer returnContainer = new(XmlRpcReturn);
            return returnContainer.VmTemplateModelList;
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.GetVirtualMachineInformation(int)"/>
        public VmModel GetVirtualMachineInformation(int VirtualMachineId)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmInfo(_Session, VirtualMachineId);
            VmInfoReturnContainer returnContainer = new(XmlRpcReturn);
            return returnContainer.VmModel;
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.PerformVirtualMachineAction(string, int)"/>
        public bool PerformVirtualMachineAction(string Action, int VirtualMachineId)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmInfo(_Session, VirtualMachineId);
            return (bool)XmlRpcReturn[0];
        }
    }
}
