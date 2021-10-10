using ScalableTeaching.DTO;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ScalableTeaching.Helpers
{
    public class SSOHelper
    {
        public async static Task<UserDTO> GetSSOData(SSOTokenDTO tokendata)
        {
            try
            {
                using var client = new HttpClient();
                var result = await client.GetAsync($"https://sso.sdu.dk/serviceValidate?ticket={tokendata.Token}&service={tokendata.ServiceEndpoint}");
                result.EnsureSuccessStatusCode();

                Console.WriteLine(await result.Content.ReadAsStringAsync());
                return await ResolveSSOReponse(result);
            }
            catch (HttpRequestException e)
            {
                Console.Out.WriteLine(e.StackTrace);
                throw new ArgumentException("SSO Response not a sucess return code", e);
            }
        }
        private async static Task<UserDTO> ResolveSSOReponse(HttpResponseMessage ResponseData)
        {
            XmlDocument document = new();
            string XmlBody = await ResponseData.Content.ReadAsStringAsync();
            document.LoadXml(XmlBody);
            if (document.GetElementsByTagName("cas:authenticationFailure").Count != 0)
            {
                throw new ArgumentException("Authentication Failed");
            }
            else
            {
                UserDTO User;
                try
                {
                    User = new UserDTO
                    {
                        Username = GetValueFromXml(document, "cas:user"),
                        Mail = GetValueFromXml(document, "mail"),
                        Sn = GetValueFromXml(document, "sn"),
                        Gn = GetValueFromXml(document, "gn"),
                        Cn = GetValueFromXml(document, "cn")
                    };
                }
                catch (NullReferenceException)
                {
                    throw new ArgumentException("Invalid response from SSO Server - Missing required tag");
                }
                return User;
            }
        }

        private static String GetValueFromXml(XmlDocument xml, String SearchString)
        {
            return xml.GetElementsByTagName(SearchString).Item(0).InnerText;
        }
    }
}
