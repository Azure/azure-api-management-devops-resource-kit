// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities
{
    public class AppInsightsUpdater
    {
        public void UpdateAppInsightNameAndInstrumentationKey(
            CreatorConfig creatorConfig, string appInsightsInstrumentationKey, string appInsightsName)
        {
            string appInsightNamePassed = string.Empty;

            if (appInsightsName != null && !string.IsNullOrEmpty(appInsightsName))
            {
                appInsightNamePassed = appInsightsName;
                foreach (APIConfig aPIConfig in creatorConfig.apis)
                {
                    if (aPIConfig.diagnostic != null)
                    {
                        aPIConfig.diagnostic.LoggerId = appInsightNamePassed;
                    }
                }
            }

            if (appInsightsInstrumentationKey != null && !string.IsNullOrEmpty(appInsightsInstrumentationKey))
            {
                string appInsightsInstrumentationKeyPassed = appInsightsInstrumentationKey;
                if (creatorConfig.loggers != null && creatorConfig.loggers.Count > 0)
                {
                    if (!string.IsNullOrEmpty(appInsightNamePassed))
                    {
                        creatorConfig.loggers[0].name = appInsightNamePassed;
                    }
                    if (creatorConfig.loggers[0].Credentials != null)
                    {
                        creatorConfig.loggers[0].Credentials.InstrumentationKey = appInsightsInstrumentationKeyPassed;
                    }
                }
            }
        }
    }
}
