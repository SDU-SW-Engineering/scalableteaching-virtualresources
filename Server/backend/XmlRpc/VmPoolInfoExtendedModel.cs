using System;
using System.Collections.Generic;
using System.Xml;
using Backend.XmlRpc;

namespace Backend.XmlRpc
{
    public class VmPoolInfoExtendedModel : XmlRpcModel
    {
        readonly List<VmModel> VmModelList = new();
        private VmPoolInfoExtendedModel() { }

        protected override bool Parse<T>(object[] input)
        {
            XmlDocument ResultDocument;
            try
            {
                ResultDocument = GetCleanXmlDocument(input);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            
            XmlNodeList VmTemplateNodes = ResultDocument.GetElementsByTagName("VM");

            foreach (XmlNode VmNode in VmTemplateNodes)
            {
                var monitoringNode = VmNode.SelectSingleNode("MONITORING");
                VmModelList.Add(new VmModel(
                    int.Parse(VmNode.SelectSingleNode("ID").InnerText),
                    VmNode.SelectSingleNode("NAME").InnerText,
                    DateTimeOffset.FromUnixTimeSeconds(long.Parse(VmNode.SelectSingleNode("LAST_POLL").InnerText)),
                    (MachineState)int.Parse(VmNode.SelectSingleNode("STATE").InnerText),
                    Decimal.Parse(monitoringNode.SelectSingleNode("CPU").InnerText),
                    int.Parse(monitoringNode.SelectSingleNode("MEMORY").InnerText),
                    monitoringNode.SelectSingleNode("GUEST_IP_ADDRESSES").InnerText.Split(',')[0],
                    VmNode.SelectSingleNode("TEMPLATE").SelectSingleNode("NIC").SelectSingleNode("MAC").InnerText
                    ));
            }
            return true;
        }

        public bool Succeded()
        {
            throw new NotImplementedException();
        }
    }
}