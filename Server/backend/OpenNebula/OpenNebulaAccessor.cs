﻿using CookComputing.XmlRpc;
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

        /// <inheritdoc cref="IOpenNebulaAccessor.CreateVirtualMachine(int, string)"/>
        public (bool, int) CreateVirtualMachine(int TemplateId, string VirtualMachineName)
        {
            Object[] XmlRpcReturn = VmTemplateManagementProxy.VmTemplateInstantiate(_Session, TemplateId, VirtualMachineName);
            return ((bool)XmlRpcReturn[0], (int)XmlRpcReturn[1]);
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.GetVirtualMachineInformation(int)"/>
        public List<VmModel> GetAllVirtualMachineInfo(bool IncludeDoneMachines, int OwnershipMask)
        {
            Object[] XmlRpcReturn = VmManagementProxy.VmPoolInfoExtended(_Session, -2, -1, -1, IncludeDoneMachines ? -2 : -1);
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