using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.XML_RPC_DTO
{
    public interface IVmManagement : IXmlRpcProxy
    {
        /// <summary>
        /// Retrieves the information about the specified virtualmachine 
        /// </summary>
        /// <param name="Session">username:password</param>
        /// <param name="VirtualMachineId">Id of the virtualmachine</param>
        /// <returns></returns>
        [XmlRpcMethod("one.vm.info")]
        Object[] VmInfo(string Session, int VirtualMachineId);
    }
}
