using ScalableTeaching.OpenNebula.Models;
using System.Collections.Generic;
using System.Xml;

namespace ScalableTeaching.OpenNebula.Containers
{
    public class VmTemplatepoolInfoReturnContainer : OpenNebulaReturnContainer
    {
        public readonly List<VmTemplateModel> VmTemplateModelList = new();
        private VmTemplatepoolInfoReturnContainer() { }

        public VmTemplatepoolInfoReturnContainer(object[] input)
        {
            var ResultDocument = GetCleanXmlDocument(input);

            XmlNodeList VmTemplateNodes = ResultDocument.GetElementsByTagName("VMTEMPLATE");

            foreach (XmlNode VmTemplateNode in VmTemplateNodes)
            {
                var templateNode = VmTemplateNode.SelectSingleNode("TEMPLATE");

                VmTemplateModelList.Add(new VmTemplateModel(
                    int.Parse(VmTemplateNode.SelectSingleNode("ID").InnerText),
                    VmTemplateNode.SelectSingleNode("NAME").InnerText,
                    templateNode.SelectSingleNode("DESCRIPTION").InnerText,
                    int.Parse(templateNode.SelectSingleNode("CPU").InnerText),
                    int.Parse(templateNode.SelectSingleNode("MEMORY").InnerText)
                    ));
            }
        }
    }
}