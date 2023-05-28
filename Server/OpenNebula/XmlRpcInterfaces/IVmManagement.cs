using CookComputing.XmlRpc;
using System;

namespace ScalableTeaching.OpenNebula.XmlRpcInterfaces
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
        
        /// <summary>
        /// Resizes a disk of a virtual machine
        /// </summary>
        /// <param name="Session">username:password</param>
        /// <param name="VirtualMachineId">Id of the virtualmachine</param>
        /// <param name="DiskId">The disk ID as stated in OpenNebula (Default 0 for the relevant machines)</param>
        /// <param name="NewSize">The size in bytes represented as a string</param>
        /// <returns></returns>
        [XmlRpcMethod("one.vm.diskresize")]
        Object[] VmDiskResize(string Session, int VirtualMachineId, int DiskId, string NewSize);
    }


}
