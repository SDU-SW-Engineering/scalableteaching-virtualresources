using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.XmlRpc
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

        /// <summary>
        /// Performs an action on a virtual machine.
        /// <para>Actions:</para>
        /// <list type="bullet">
        ///     <item>terminate-hard</item>
        ///     <item>terminate</item>
        ///     <item>undeploy-hard</item>
        ///     <item>undeploy</item>
        ///     <item>poweroff-hard</item>
        ///     <item>poweroff</item>
        ///     <item>reboot-hard</item>
        ///     <item>reboot</item>
        ///     <item>hold</item>
        ///     <item>release</item>
        ///     <item>stop</item>
        ///     <item>suspend</item>
        ///     <item>resume</item>
        ///     <item>resched</item>
        ///     <item>unresched</item>
        /// </list>
        /// </summary>
        /// <param name="Session">username:password</param>
        /// <param name="Action">Any of the actions listed above</param>
        /// <param name="VirtualMachineId">Id of the virtualmachine</param>
        /// <returns></returns>
        [XmlRpcMethod("one.vm.action")]
        Object[] VmAction(string Session, string Action, int VirtualMachineId);

        /// <summary>
        /// Retrieves information for all virtual machines
        /// <para>Furter details on flags: https://cutt.ly/gnEqdjl</para>
        /// </summary>
        /// <param name="Session">username:password</param>
        /// <param name="OwnerFilterFlag">-2 for all resources</param>
        /// <param name="IdLowerBound">-1 for no lower bound</param>
        /// <param name="IdUpperBound">-1 for no upper bound</param>
        /// <param name="StateFilterFlag">-1 for all existing machines, -2 for all machines (Includes terminated machines)</param>
        /// <returns></returns>
        [XmlRpcMethod("one.vmpool.infoextended")]
        Object[] VmPoolInfoExtended(string Session, int OwnerFilterFlag, int IdLowerBound, int IdUpperBound, int StateFilterFlag);
    }


}
