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
        public (bool, int) CreateVirtualMachine(int TemplateId, string VirtualMachineName, int memory = 1024, int vcpu = 1, int storage = 16384)
        {
            if (!(memory % 1024 == 0 && memory <= 8192 && memory >= 1024)) throw new ArgumentException("The given memmory value was invalid. Memmory must be a multiple of 1024 and at max 8192");
            if (!(vcpu >= 1 && vcpu <= 8)) throw new ArgumentException("The given cpu count was invalid. cpu count must be a natural number no greater than 8");
            if (!(storage >= 16384 && storage <= 51200)) throw new ArgumentException("The given strage value was dinvalid. ");

            //TODO: Disk attribute restricted \nDISK=[TYPE=fs,SIZE={storage},FORMAT=raw,DEV_PREFIX=sd,TARGET=sda] &lt;DISK&gt;&lt;TYPE&gt;fs&lt;/TYPE&gt;&lt;SIZE&gt;8192&lt;/SIZE&gt;&lt;FORMAT&gt;raw&lt;/FORMAT&gt;&lt;DEV_PREFIX&gt;sd&lt;/DEV_PREFIX&gt;&lt;TARGET&gt;sda&lt;/TARGET&gt;&lt;/DISK&gt;
            string TemplateString = $"<template><MEMORY>{memory}</MEMORY><VCPU>{vcpu}</VCPU></template>";
            Object[] XmlRpcReturn = VmTemplateManagementProxy.VmTemplateInstantiate(_Session, TemplateId, VirtualMachineName, false, TemplateString);
            //TODO: remove this
            if(XmlRpcReturn[1].GetType() == "".GetType())
            {
                Console.WriteLine($"Error in machine creation: {XmlRpcReturn[1]}");
                throw new Exception($"Error in machine creation: {XmlRpcReturn[1]}");
            }

            return ((bool)XmlRpcReturn[0], (int)XmlRpcReturn[1]);
        }

        /// <inheritdoc cref="IOpenNebulaAccessor.ResizeVirtualMachine(bool, string)(int, int)"/>
        public (bool, string) ResizeVirtualMachine(int VirtualMachineId, int Bytes = 16384)
        {
            if (!(Bytes >= 16384 && Bytes <= 51200)) throw new ArgumentException("The given Bytes value is invalid. ");
            Object[] XmlRpcReturn = VmManagementProxy.VmDiskResize(_Session, VirtualMachineId, 0, Bytes.ToString());
            if(XmlRpcReturn[1].GetType() == "".GetType())
            {
                Console.WriteLine($"Error in machine creation: {XmlRpcReturn[1]}");
                return ((bool)XmlRpcReturn[0], (string)XmlRpcReturn[1]);
            }
            return ((bool)XmlRpcReturn[0], "Success");
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
            Object[] XmlRpcReturn = VmManagementProxy.VmAction(_Session, Action, VirtualMachineId);
            return (bool)XmlRpcReturn[0];
        }
    }
}
