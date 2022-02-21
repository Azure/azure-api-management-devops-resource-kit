using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Caching.Memory;
using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy
{
    static class PolicyTemplateUtils
    {
        static readonly IMemoryCache PolicyCache = new MemoryCache(new MemoryCacheOptions());

        public static string GetPolicyContent(ExtractorParameters extractorParameters, PolicyTemplateResource policyTemplateResource)
        {
            // the backend is used in a policy if the xml contains a set-backend-service policy, which will reference the backend's url or id
            string policyContent = policyTemplateResource.Properties.Value;

            // check if this is a file or is it the raw policy content
            if (policyContent.Contains(".xml"))
            {
                var key = policyContent;
                //check cache
                if (PolicyCache.TryGetValue(key, out string content))
                {
                    return content;
                }

                var filename = policyContent.Split(',')[1].Replace("'", string.Empty).Trim();
                var policyFolder = $@"{extractorParameters.FilesGenerationRootDirectory}/policies";
                var filepath = $@"{Directory.GetCurrentDirectory()}/{policyFolder}/{filename}";

                if (File.Exists(filepath))
                {
                    policyContent = File.ReadAllText(filepath);
                    PolicyCache.Set(key, policyContent);
                }
            }

            return policyContent;
        }
    }
}
