using ScalableTeaching.OpenNebula;
using System;

namespace OpenNebulaTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IOpenNebulaAccessor accessor = new OpenNebulaAccessor(Environment.GetEnvironmentVariable("OpenNebulaLocation"), Environment.GetEnvironmentVariable("OpenNebulaSession"));
            var returnval = accessor.GetAllVirtualMachineTemplateInfo();
            foreach (var val in returnval)
            {
                Console.WriteLine($"Name: {val.TemplateName}, Id: {val.TemplateId}");
            }
        }
    }
}
