// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger.Cache
{
    public class LoggersCache
    {
        public IDictionary<string, string> ServiceLevelDiagnosticLoggerBindings = new Dictionary<string, string>();

        public IDictionary<string, ISet<DiagnosticLoggerBinding>> ApiDiagnosticLoggerBindings { get; set; } = new Dictionary<string, ISet<DiagnosticLoggerBinding>>();

        internal Dictionary<string, object> CreateResultingMap()
        {
            // TODO this is unrefactored and left as it is from legacy code
            // https://github.com/Azure/azure-api-management-devops-resource-kit/issues/617

            var resultingMap = new Dictionary<string, object>();

            foreach (var (key, value) in this.ServiceLevelDiagnosticLoggerBindings)
            {
                if (!resultingMap.ContainsKey(key))
                {
                    resultingMap.Add(key, value);
                }
            }

            foreach (var (key, bindings) in this.ApiDiagnosticLoggerBindings)
            {
                var bindingsMap = new Dictionary<string, string>();
                foreach (var binding in bindings)
                {
                    bindingsMap.Add(binding.DiagnosticName, binding.LoggerId);
                }

                if (!resultingMap.ContainsKey(key))
                {
                    resultingMap.Add(key, bindingsMap);
                }
            }

            return resultingMap;
        }
    }
}
