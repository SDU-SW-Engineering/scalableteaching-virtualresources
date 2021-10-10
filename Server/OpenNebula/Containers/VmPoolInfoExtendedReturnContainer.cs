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

                var monitoringNode = VmNode.SelectSingleNode("MONITORING");
                if( monitoringNode == null)
                {
                    Console.WriteLine("Error getting Monitoring node trashing result");
                    continue;
                }

                result &= int.TryParse(VmNode.SelectSingleNode("ID")?.InnerText, out int machineId);

                if (!result)
                {
                    Console.WriteLine("Error getting ID Node trashing result");
                    continue;
                }

                var machineName = VmNode.SelectSingleNode("NAME")?.InnerText;
                result &= machineName != null;
                if (!result)
                {
                    Console.WriteLine("Error getting Name node trashing result");
                    continue;
                }

                result &= long.TryParse(VmNode.SelectSingleNode("LAST_POLL")?.InnerText, out long lastPollLong);
                var lastPoll = DateTimeOffset.FromUnixTimeSeconds(lastPollLong);
                if (!result)
                {
                    Console.WriteLine("Error getting Last poll trashing result");
                    continue;
                }

                result &= int.TryParse(VmNode.SelectSingleNode("STATE")?.InnerText, out int machineStateInt);
                MachineStates machineState = (MachineStates)machineStateInt;
                if (!result)
                {
                    Console.WriteLine("Error getting STATE node trashing result");
                    continue;
                }

                result &= int.TryParse(VmNode.SelectSingleNode("LCM_STATE")?.InnerText, out int lcmstateint);
                LCMStates lcmState = (LCMStates)lcmstateint;

                //if (!result)
                //{
                //    Console.WriteLine("Error getting LCMSTATE Node trashing result");
                //    continue;
                //}

                result &= decimal.TryParse(monitoringNode.SelectSingleNode("CPU")?.InnerText, out decimal machineCpuPercent);
                //if (!result)
                //{
                //    Console.WriteLine("Error getting CPU Node trashing result");
                //    continue;
                //}

                result &= int.TryParse(monitoringNode.SelectSingleNode("MEMORY")?.InnerText, out int machineMemmoryUtilizationBytes);
                //if (!result)
                //{
                //    Console.WriteLine("Error getting MEMORY Node trashing result");
                //    continue;
                //}

                var machineIpNodeInnerText = monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES")?.InnerText ?? "";
                Match machineIpMatch = Regex.Match(machineIpNodeInnerText, @"(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3})");
                string machineIp;
                //Console.WriteLine($"machineIpNodeInnerText, {machineIpNodeInnerText}, node == null : {monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES") == null}");
                if (!machineIpMatch.Success)
                {
                    result = false;
                    machineIp = "";
                }
                else
                {
                    machineIp = machineIpMatch.Value;
                }
                //if (!result)
                //{
                //    Console.WriteLine("Error getting IP trashing result");
                //    continue;
                //}

                var machineMac = VmNode.SelectSingleNode("TEMPLATE")?.SelectSingleNode("NIC")?.SelectSingleNode("MAC")?.InnerText;
                result &= machineMac != null;

                //if (!result)
                //{
                //    Console.WriteLine("Error getting MAC Node trashing result");
                //    continue;
                //}

                //if (!result)
                //{
                //    Console.WriteLine("VMPoolInfoExtendedReturnContainer: Error in result - Trashing result");
                //    continue;
                //}
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
            }
        }
    }
}