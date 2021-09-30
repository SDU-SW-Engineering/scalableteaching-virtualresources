using ScalableTeaching.OpenNebula.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace ScalableTeaching.OpenNebula.Containers
{
    public class VmPoolInfoExtendedReturnContainer : OpenNebulaReturnContainer
    {
        public List<VmModel> VmModelList;

        public VmPoolInfoExtendedReturnContainer(object[] input)
        {
            Console.WriteLine("VMPoolInfoExtendedReturnContainer creation started");
            if (input is null)
            {
                Console.WriteLine("Input was null for VmPoolInfoExtendedReturnContainer");
            }

            VmModelList = new List<VmModel>();
            //Console.WriteLine("VMPoolInfoExtendedReturnContainer: VM Model List Created in ");
            XmlDocument ResultDocument;
            ResultDocument = GetCleanXmlDocument(input);
            //Console.WriteLine("VMPoolInfoExtendedReturnContainer: Clean result document generated");

            XmlNodeList VmTemplateNodes = ResultDocument.GetElementsByTagName("VM");
            //Console.WriteLine("VMPoolInfoExtendedReturnContainer: VMTemplate Nodes recovered");

            foreach (XmlNode VmNode in VmTemplateNodes)
            {
                var result = true;
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: Looping nodes");

                var monitoringNode = VmNode.SelectSingleNode("MONITORING");
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: Monitoring node selected");
                Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                result &= monitoringNode != null;

                result &= int.TryParse(VmNode.SelectSingleNode("ID")?.InnerText, out int machineId);
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: SelectedMachineID");

                var machineName = VmNode.SelectSingleNode("NAME")?.InnerText;
                Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                result &= machineName != null;
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: Selected Name Node");

                result &= long.TryParse(VmNode.SelectSingleNode("LAST_POLL")?.InnerText, out long lastPollLong);
                var lastPoll = DateTimeOffset.FromUnixTimeSeconds(lastPollLong);
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: SelectedlastPoll");

                result &= int.TryParse(VmNode.SelectSingleNode("STATE")?.InnerText, out int machineStateInt);
                MachineStates machineState = (MachineStates)machineStateInt;
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: Selected state");

                result &= int.TryParse(VmNode.SelectSingleNode("LCM_STATE")?.InnerText, out int lcmstateint);
                LCMStates lcmState = (LCMStates)lcmstateint;
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: selected lcmstate");

                result &= decimal.TryParse(monitoringNode.SelectSingleNode("CPU")?.InnerText, out decimal machineCpuPercent);
                Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: selected cpu%");

                result &= int.TryParse(monitoringNode.SelectSingleNode("MEMORY")?.InnerText, out int machineMemmoryUtilizationBytes);
                Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: selected Memmory bytes");

                var machineIp = Regex.Match(monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES")?.InnerText, @"(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})").Value;
                Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                result &= machineIp != null;
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: selected ip");

                var machineMac = VmNode.SelectSingleNode("TEMPLATE")?.SelectSingleNode("NIC")?.SelectSingleNode("MAC")?.InnerText;
                Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                result &= machineMac != null;

                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: selected mac");

                if (!result)
                {
                    Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                    continue;
                }
                VmModelList.Add(new VmModel(
                    machineId,
                    machineName,
                    lastPoll,
                    machineState,
                    lcmState,
                    machineCpuPercent,
                    machineMemmoryUtilizationBytes,
                    machineIp,
                    machineMac
                    ));
                //Console.WriteLine("VMPoolInfoExtendedReturnContainer: Added model to list");
            }
        }
    }
}