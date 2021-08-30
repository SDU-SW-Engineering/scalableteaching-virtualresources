using System;
using System.Xml;

namespace ScalableTeaching.OpenNebula.Containers
{
    public abstract class OpenNebulaReturnContainer
    {
        protected XmlDocument GetCleanXmlDocument(object[] input)
        {
            XmlDocument ResultXmlDocument;
            if (!(bool)input[0]) throw new InvalidOperationException("Request Failure");
            try
            {
                ResultXmlDocument = OpenNebulaResultStringCleaner.Clean((string)input[1]);
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException("Incorrect number of arguments in array");
            }
            return ResultXmlDocument;
        }
    }
}
