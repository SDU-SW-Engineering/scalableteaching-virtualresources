using System;

namespace ScalableTeaching.DTO
{
    public class SSOTokenDTO
    {
        public String Token { get; set; }
        public String ServiceEndpoint { get; set; }

        public override string ToString()
        {
            return $"Token: {Token}, ServiceEndpoint: {ServiceEndpoint}";
        }
    }
}
