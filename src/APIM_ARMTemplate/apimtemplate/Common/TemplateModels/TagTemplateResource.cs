using apimtemplate.Common.Templates.Abstractions;

namespace apimtemplate.Common.TemplateModels
{

    public class TagTemplateResource : APITemplateSubResource
    {
        public TagTemplateProperties properties { get; set; }
    }

    public class TagTemplateProperties
    {
        public string displayName { get; set; }
    }
}
