using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger
{

    static class LoggerTemplateUtils
    {
        public static Dictionary<string, string> GetAllLoggerResourceIds(List<TemplateResource> resources)
        {
            Dictionary<string, string> logResIds = new Dictionary<string, string>();
            foreach (LoggerTemplateResource resource in resources)
            {
                string validLoggerName = GetValidLoggerParamName(resource.Name);
                string resourceId = resource.Properties.resourceId;
                logResIds.Add(validLoggerName, resourceId);
            }
            return logResIds;
        }

        public static Template SetLoggerResourceId(Template loggerTemplate)
        {
            TemplateResource[] loggerResources = loggerTemplate.Resources.ToArray();
            List<TemplateResource> nLoggerResource = new List<TemplateResource>();
            foreach (LoggerTemplateResource resource in loggerResources)
            {
                string validLoggerName = GetValidLoggerParamName(resource.Name);
                resource.Properties.resourceId = $"[parameters('{ParameterNames.LoggerResourceId}').{validLoggerName}]";
                nLoggerResource.Add(resource);
            }
            loggerTemplate.Resources = nLoggerResource.ToArray();
            return loggerTemplate;
        }

        static string GetValidLoggerParamName(string resourceName)
        {
            string[] loggerNameStrs = resourceName.Split(new char[] { ',' });
            string validLoggerName = ParameterNamingHelper.GenerateValidParameterName(loggerNameStrs[loggerNameStrs.Length - 1], ParameterPrefix.LogResourceId);
            return validLoggerName;
        }
    }
}
