using CookComputing.XmlRpc;
using System;

namespace ScalableTeaching.OpenNebula.XmlRpcInterfaces
{
    /// <summary>
    /// Interfaces for method calls on the template
    /// </summary>
    public interface IVmTemplateManagement : IXmlRpcProxy
    {
        /// <summary>
        /// Retrieves an xml blob containing information about the templates/images available.
        /// <para>Should be used in this application with the parameters username:password,
        /// -2, -1, -1 to get entire list of templates</para>
        /// </summary>
        /// <param name="Session">Username:Password</param>
        /// <param name="OwnerFilterFlag"><list type="table">
        /// <listheader>Owner Filtering</listheader>
        /// <item><term>-4</term><description> Resources belonging to the user’s primary group.</description></item>
        /// <item><term>-3</term><description> Resources belonging to the user.</description></item>
        /// <item><term>-2</term><description> All resources.</description></item>
        /// <item><term>-1</term><description> Resources belonging to the user and any of his groups.</description></item>
        /// </list></param>
        /// <param name="LowerBound">When the next parameter is >= -1 this is the Range start ID. Can be -1. For smaller values this is the offset used for pagination.</param>
        /// <param name="UpperBound">For Values &gt;= -1 this is the range end ID. Can be -1 to get until the last ID. For values &lt;-1 this is the page size used for pagination.</param>
        /// <returns></returns>
        [XmlRpcMethod("one.templatepool.info")]
        Object[] VmTemplatePoolInfo(string Session, int OwnerFilterFlag, int LowerBound, int UpperBound);

        /// <summary>
        /// Creates an instance of a virtual machine based on the TemplateId
        /// </summary>
        /// <param name="Session">username:password</param>
        /// <param name="TemplateId">Id of template</param>
        /// <param name="VirtualMachineName">Name for the virtual machine to be</param>
        /// <param name="CreateOnHold">true if machine should be put on hold after creation</param>
        /// <param name="TemplateCustomizations">String containing customizations for the template <see href="https://docs.opennebula.io/6.0/integration_and_development/system_interfaces/api.html#one-template-instantiate"/></param>
        /// <returns></returns>
        [XmlRpcMethod("one.template.instantiate")]
        Object[] VmTemplateInstantiate(string Session, int TemplateId, string VirtualMachineName, bool CreateOnHold, string TemplateCustomizations);
    }

}

