namespace ScalableTeaching.OpenNebula.Models
{
    public struct VmTemplateModel
    {
        public readonly int TemplateId { get; }
        public readonly string TemplateName { get; }
        public readonly string Description { get; }
        public readonly int TemplateCpuCores { get; }
        public readonly int TemplateStorageMegabytes { get; }


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
