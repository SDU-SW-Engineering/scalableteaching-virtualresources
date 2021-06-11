using System;
using System.Collections.Generic;
using System.Xml;
using Backend.XmlRpc;

namespace Backend.XmlRpc
{
    public class VmTemplatepoolInfoModel : XmlRpcModel
    {
        readonly List<VmTemplateModel> VmTemplateModelList = new();
        private VmTemplatepoolInfoModel() { }

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
            return true;
        }

        public bool Succeded()
        {
            throw new NotImplementedException();
        }
    }
}