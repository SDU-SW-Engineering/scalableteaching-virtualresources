using System;
using System.Collections.Generic;
using System.Xml;
using ScalableTeaching.OpenNebula.Models;

namespace ScalableTeaching.OpenNebula.Containers
{
    public class VmPoolInfoExtendedReturnContainer : OpenNebulaReturnContainer
    {
        public readonly List<VmModel> VmModelList = new();

        public VmPoolInfoExtendedReturnContainer(object[] input)
        {
            XmlDocument ResultDocument;
            ResultDocument = GetCleanXmlDocument(input);
            
            XmlNodeList VmTemplateNodes = ResultDocument.GetElementsByTagName("VM");

            foreach (XmlNode VmNode in VmTemplateNodes)
            {
                var monitoringNode = VmNode.SelectSingleNode("MONITORING");
                VmModelList.Add(new VmModel(
                    int.Parse(VmNode.SelectSingleNode("ID").InnerText),
                    VmNode.SelectSingleNode("NAME").InnerText,
                    DateTimeOffset.FromUnixTimeSeconds(long.Parse(VmNode.SelectSingleNode("LAST_POLL").InnerText)),
                    (MachineStates)int.Parse(VmNode.SelectSingleNode("STATE").InnerText),
                    (LCMStates)int.Parse(VmNode.SelectSingleNode("LCM_STATE").InnerText),
                    Decimal.Parse(monitoringNode.SelectSingleNode("CPU").InnerText),
                    int.Parse(monitoringNode.SelectSingleNode("MEMORY").InnerText),
                    monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES").InnerText.Split(',')[0],
                    VmNode.SelectSingleNode("TEMPLATE").SelectSingleNode("NIC").SelectSingleNode("MAC").InnerText
                    ));
            }
        }
    }
}