using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Backend.XmlRpc
{
    public abstract class XmlRpcModel
    {
        protected bool _succeded;
        protected XmlRpcModel() { }
        public static T BuildXmlRpcModel<T>(Object[] input) where T : XmlRpcModel, new()
        {
            var instance = new T();
            instance.Parse<T>(input);
            return instance;

        }
        protected abstract bool Parse<T>(object[] input) where T : XmlRpcModel;
        protected XmlDocument GetCleanXmlDocument(object[] input)
        {
            XmlDocument ResultXmlDocument;
            if (!(bool)input[0]) throw new InvalidOperationException("Request Failure");
            try
            {
                ResultXmlDocument = XmlRpcResultStringCleaner.Clean((string)input[1]);
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException("Incorrect number of arguments in array");
            }
            return ResultXmlDocument;
        }
        public bool Succeded()
        {
            return _succeded;
        }
    }
}
