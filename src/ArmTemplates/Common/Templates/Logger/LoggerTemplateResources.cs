// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger
{
    public class LoggerTemplateResources : ITemplateResources
    {
        public List<LoggerTemplateResource> Loggers { get; set; } = new();

        public IDictionary<string, string> LoggerResourceIds { get; private set; }

        public TemplateResource[] BuildTemplateResources()
        {
            return this.Loggers.ToArray();
        }

        public bool HasContent()
        {
            return !this.Loggers.IsNullOrEmpty();
        }

        public void FormAllLoggerResourceIdsCache()
        {
            Dictionary<string, string> logResIds = new Dictionary<string, string>();
            foreach (var resource in this.Loggers)
            {
                var validLoggerName = GetValidLoggerParamName(resource.Name);
                var resourceId = resource.Properties.ResourceId;
                logResIds.Add(validLoggerName, resourceId);
            }

            this.LoggerResourceIds = logResIds;
        }

        public void SetLoggerResourceIdForEachLogger()
        {
            foreach (var resource in this.Loggers)
            {
                var validLoggerName = GetValidLoggerParamName(resource.Name);
                resource.Properties.ResourceId = $"[parameters('{ParameterNames.LoggerResourceId}').{validLoggerName}]";
            }
        }

        static string GetValidLoggerParamName(string resourceName)
        {
            var loggerNameStrs = resourceName.Split(new char[] { ',' });
            var validLoggerName = ParameterNamingHelper.GenerateValidParameterName(loggerNameStrs[loggerNameStrs.Length - 1], ParameterPrefix.LogResourceId);
            return validLoggerName;
        }
    }
}
