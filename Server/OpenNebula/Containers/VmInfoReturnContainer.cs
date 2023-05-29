using ScalableTeaching.OpenNebula.Models;
using System;
using System.Xml;

namespace ScalableTeaching.OpenNebula.Containers
{
    public class VmInfoReturnContainer : OpenNebulaReturnContainer
    {
        public VmModel VmModel { get; private set; }

        public VmInfoReturnContainer(object[] input)
        {
            XmlDocument ResultDocument;
            ResultDocument = GetCleanXmlDocument(input);

            XmlNode VmNode = ResultDocument.SelectSingleNode("VM");
            var monitoringNode = VmNode.SelectSingleNode("MONITORING");


#nullable enable
            //Critical fields
            string? machineIdInnerText = VmNode.SelectSingleNode("ID")!.InnerText;
            if (machineIdInnerText == null) throw new FieldNotFoundException("ID field not found");
            int machineId = int.Parse(machineIdInnerText);

            string? name = VmNode.SelectSingleNode("NAME")?.InnerText;
            if (name == null) throw new FieldNotFoundException("NAME field not found");

            string? lastPollInnerText = VmNode.SelectSingleNode("LAST_POLL")?.InnerText;
            if (lastPollInnerText == null) throw new FieldNotFoundException("LAST_POLL field not found");
            DateTimeOffset lastPoll = DateTimeOffset.FromUnixTimeSeconds(long.Parse(lastPollInnerText));

            string? machineStateInnerText = VmNode.SelectSingleNode("STATE")?.InnerText;
            if (machineStateInnerText == null) throw new FieldNotFoundException("STATE field not found");
            MachineStates machineState = (MachineStates)int.Parse(machineStateInnerText);

            //Optional fields
            string? lcmStateInnerText = VmNode.SelectSingleNode("LCM_STATE")?.InnerText;
            LCMStates? lCMState = lcmStateInnerText == null ? null : (LCMStates)int.Parse(lcmStateInnerText);

            string? machineCpuUtilizationPercentInnerText = monitoringNode!.SelectSingleNode("CPU")?.InnerText;
            decimal? machineCpuUtilizationPercent = machineCpuUtilizationPercentInnerText == null ? null 
                : decimal.Parse(machineCpuUtilizationPercentInnerText);

            string? machineMemmoryUtilizationBytesInnerText = monitoringNode.SelectSingleNode("MEMORY")?.InnerText;
            int? machineMemmoryUtilizationBytes = machineMemmoryUtilizationBytesInnerText == null ? null 
                : int.Parse(machineMemmoryUtilizationBytesInnerText);

            string? machineIp = monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES")?.InnerText.Split(',')[0];

            string? machineMac = VmNode.SelectSingleNode("TEMPLATE")?.SelectSingleNode("NIC")?.SelectSingleNode("MAC")?.InnerText;

            VmModel = new VmModel(
                machineId,
                name,
                lastPoll,
                machineState,
                lCMState,
                machineCpuUtilizationPercent,
                machineMemmoryUtilizationBytes,
                machineIp,
                machineMac
                );
#nullable restore
        }
    }
}