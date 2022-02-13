using apimtemplate.Common.Constants;
using apimtemplate.Common.Helpers;
using apimtemplate.Common.Templates.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace apimtemplate.Common.Templates.Logger
{

    internal static class LoggerTemplateUtils
    {
        public static Dictionary<string, string> GetAllLoggerResourceIds(List<TemplateResource> resources)
        {
            Dictionary<string, string> logResIds = new Dictionary<string, string>();
            foreach (LoggerTemplateResource resource in resources)
            {
                string validLoggerName = GetValidLoggerParamName(resource.name);
                string resourceId = resource.properties.resourceId;
                logResIds.Add(validLoggerName, resourceId);
            }
            return logResIds;
        }

        public static Template SetLoggerResourceId(Template loggerTemplate)
        {
            TemplateResource[] loggerResources = loggerTemplate.resources.ToArray();
            List<TemplateResource> nLoggerResource = new List<TemplateResource>();
            foreach (LoggerTemplateResource resource in loggerResources)
            {
                string validLoggerName = GetValidLoggerParamName(resource.name);
                resource.properties.resourceId = $"[parameters('{ParameterNames.LoggerResourceId}').{validLoggerName}]";
                nLoggerResource.Add(resource);
            }
            loggerTemplate.resources = nLoggerResource.ToArray();
            return loggerTemplate;
        }

        private static string GetValidLoggerParamName(string resourceName)
        {
            string[] loggerNameStrs = resourceName.Split(new char[] { ',' });
            string validLoggerName = ParameterNamingHelper.GenerateValidParameterName(loggerNameStrs[loggerNameStrs.Length - 1], ParameterPrefix.LogResourceId);
            return validLoggerName;
        }
    }
}
