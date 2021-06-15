using CookComputing.XmlRpc;
using ScalableTeaching.OpenNebula.Containers;
using ScalableTeaching.OpenNebula.Models;
using ScalableTeaching.OpenNebula.XmlRpcInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScalableTeaching.OpenNebula
{
    public class OpenNebulaAccessor : IOpenNebulaAccessor
    {
        private IVmManagement VmManagementProxy;
        private IVmTemplateManagement VmTemplateManagementProxy;
        private string _Session;

        public OpenNebulaAccessor(string ServerUrl, string Session)
        {
            VmManagementProxy = XmlRpcProxyGen.Create<IVmManagement>();
            VmManagementProxy.Url = ServerUrl;
            VmTemplateManagementProxy = XmlRpcProxyGen.Create<IVmTemplateManagement>();
            VmTemplateManagementProxy.Url = ServerUrl;
            _Session = Session;
        }

        public (bool, int) CreateVirtualMachine(int TemplateId, string VirtualMachineName)
        {
            Object[] XmlRpcReturn = VmTemplateManagementProxy.VmTemplateInstantiate(_Session, TemplateId, VirtualMachineName);
            return ((bool)XmlRpcReturn[0], (int)XmlRpcReturn[1]);
        }

        public List<VmModel> GetAllVirtualMachineInfo(bool IncludeDoneMachines)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmPoolInfoExtended(_Session, -2, -1, -1, IncludeDoneMachines ? -2 : -1);
            VmPoolInfoExtendedReturnContainer returnContainer = new(XmlRpcReturn);
            return returnContainer.VmModelList;
        }

        public List<VmTemplateModel> GetAllVirtualMachineTemplateInfo()
        {
            Object[] XmlRpcReturn = VmTemplateManagementProxy.VmTemplatePoolInfo(_Session, -2, -1, -1);
            VmTemplatepoolInfoReturnContainer returnContainer = new(XmlRpcReturn);
            return returnContainer.VmTemplateModelList;
        }

        public VmModel GetVirtualMachineInformation(int VirtualMachineId)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmInfo(_Session, VirtualMachineId);
            VmInfoReturnContainer returnContainer = new(XmlRpcReturn);
            return returnContainer.VmModel;
        }

        public bool PerformVirtualMachineAction(string Action, int VirtualMachineId)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmInfo(_Session, VirtualMachineId);
            return (bool)XmlRpcReturn[0];
        }
    }
}
