using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.XmlRpc
{
    public struct VmTemplateModel
    {
        public int TemplateId { get; }
        public string TemplateName { get; }
        public string Description { get; }
        public int TemplateCpuCores { get; }
        public int TemplateStorageMegabytes { get; }


        public VmTemplateModel(
            int templateId,
            string templateName,
            string description,
            int templateCpuCores,
            int templateStorageMegabytes)
        {
            TemplateId = templateId;
            TemplateName = templateName;
            Description = description;

            TemplateCpuCores = templateCpuCores;
            TemplateStorageMegabytes = templateStorageMegabytes;
        }
    }
}
