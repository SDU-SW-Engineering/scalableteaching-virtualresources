using System;
using System.Xml;
using ScalableTeaching.OpenNebula.Models;

namespace ScalableTeaching.OpenNebula.Containers
{
    public class VmInfoReturnContainer : OpenNebulaReturnContainer
    {
        public VmModel VmModel { get; private set; }

        public VmInfoReturnContainer (object[] input)
        {
            XmlDocument ResultDocument;
            ResultDocument = GetCleanXmlDocument(input);

            XmlNode VmNode = ResultDocument.SelectSingleNode("VM");
            var monitoringNode = VmNode.SelectSingleNode("MONITORING");
            VmModel = new VmModel(
                int.Parse(VmNode.SelectSingleNode("ID").InnerText),
                VmNode.SelectSingleNode("NAME").InnerText,
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(VmNode.SelectSingleNode("LAST_POLL").InnerText)),
                (MachineStates)int.Parse(VmNode.SelectSingleNode("STATE").InnerText),
                (LCMStates)int.Parse(VmNode.SelectSingleNode("LCM_STATE").InnerText),
                Decimal.Parse(monitoringNode.SelectSingleNode("CPU").InnerText),
                int.Parse(monitoringNode.SelectSingleNode("MEMORY").InnerText),
                monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES").InnerText.Split(',')[0],
                VmNode.SelectSingleNode("TEMPLATE").SelectSingleNode("NIC").SelectSingleNode("MAC").InnerText
                );
        }
    }
}