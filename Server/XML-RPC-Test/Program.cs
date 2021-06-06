using System;
using CookComputing.XmlRpc;

namespace commstest
{
    public struct fuck
    {
        public Object bob;
    }
    public struct TemplatePool
    {
        public bool Success;
        public string Value;
        public int ErrorCode;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int ErrorId;
    }

    [XmlRpcUrl("http://localhost:2633/RPC2")]
    public interface ITemplatePool : IXmlRpcProxy
    {
        [XmlRpcMethod("one.templatepool.info")]
        Object[] TemplatePoolInfo(string session, int filter, int min, int max);
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ITemplatePool proxy = XmlRpcProxyGen.Create<ITemplatePool>();
            Console.WriteLine("Proxy Created");
            Object[] result = proxy.TemplatePoolInfo("stapi:6TrSG73m4DGOKPDu", -2, -1, -1);
            Console.WriteLine(result.Length);
            var templatePool = new TemplatePool();
            if (result.Length == 3)
            {
                Console.WriteLine(((string)(result[1])).Replace("&gt;", ">").Replace("&lt;", "<"));
            }
        }
    }
}