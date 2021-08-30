using System.Xml;

namespace ScalableTeaching.OpenNebula
{
    public class OpenNebulaResultStringCleaner
    {
        public static XmlDocument Clean(string DirtyXmlString)
        {
            string CleanString = DirtyXmlString.Replace("&gt;", ">").Replace("&lt;", "<");
            XmlDocument ReturnXmlDoc = new();
            ReturnXmlDoc.LoadXml(CleanString);
            return ReturnXmlDoc;
        }
    }
}
