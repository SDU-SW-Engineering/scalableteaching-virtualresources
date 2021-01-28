using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTO
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
