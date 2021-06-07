using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CookComputing.XmlRpc;

namespace Backend.XML_RPC_DTO
{
    internal interface IVmTemplate : IXmlRpcProxy
    {
        /// <summary>
        /// Retrieves an xml blob containing information about the templates/images available
        /// Should be used in this application with the parameters username:password, -2, -1, -1 to get entire list of templates
        /// </summary>
        /// <param name="Session">Username:Password</param>
        /// <param name="FilterFlag">-4: Resources belonging to the user’s primary group\n-3: Resources belonging to the user\n-2: All resources\n-1: Resources belonging to the user and any of his groups\n>= 0: UID User’s Resources</param>
        /// <param name="LowerBound">When the next parameter is >= -1 this is the Range start ID. Can be -1. For smaller values this is the offset used for pagination.</param>
        /// <param name="UpperBound">For values >= -1 this is the Range end ID. Can be -1 to get until the last ID. For values < -1 this is the page size used for pagination.</param>
        /// <returns></returns>
        [XmlRpcMethod("one.templatepool.info")]
        Object[] VmTemplatePoolInfo(string Session, int FilterFlag, int LowerBound, int UpperBound);

        /// <summary>
        /// Creates an instance of a virtual machine based on the TemplateId
        /// </summary>
        /// <param name="Session">username:password</param>
        /// <param name="TemplateId">Id of template</param>
        /// <param name="VirtualMachineName">Name for the virtual machine to be</param>
        /// <returns></returns>
        [XmlRpcMethod("one.templatepool.instantiate")]
        Object[] VmTemplateInstantiate(string Session, int TemplateId, string VirtualMachineName);
    }

}
