using apimtemplate.Common.Templates.Abstractions;
using System.Collections.Generic;

namespace apimtemplate.Common.TemplateModels
{
    public class MasterTemplateResource : TemplateResource
    {
        public MasterTemplateProperties properties { get; set; }
    }

    public class MasterTemplateProperties
    {
        public string mode { get; set; }
        public MasterTemplateLink templateLink { get; set; }
        public Dictionary<string, TemplateParameterProperties> parameters { get; set; }
    }

    public class MasterTemplateLink
    {
        public string uri { get; set; }
        public string contentVersion { get; set; }
    }
}
