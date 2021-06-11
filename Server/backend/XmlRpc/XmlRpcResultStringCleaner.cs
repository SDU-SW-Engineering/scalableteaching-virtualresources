using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Backend.XmlRpc
{
    public class XmlRpcResultStringCleaner
    {
        public static XmlDocument Clean(string DirtyXmlString)
        {
            string CleanString = DirtyXmlString.Replace("&gt;", ">").Replace("&lt;","<");
            XmlDocument ReturnXmlDoc = new XmlDocument();
            ReturnXmlDoc.LoadXml(CleanString);
            return ReturnXmlDoc;
        }
    }
}
