using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace apimtemplate.Creator.Utilities
{
    public class AppInsightsUpdater
    {
        public void UpdateAppInsightNameAndInstrumentationKey(
            CreatorConfig creatorConfig, CommandOption appInsightsInstrumentationKey, CommandOption appInsightsName)
        {
            string appInsightNamePassed = string.Empty;

            if(appInsightsName != null && !string.IsNullOrEmpty(appInsightsName.Value()))
            {
                appInsightNamePassed = appInsightsName.Value();
                foreach (APIConfig aPIConfig in creatorConfig.apis)
                {
                    if(aPIConfig.diagnostic != null)
                    {
                        aPIConfig.diagnostic.loggerId = appInsightNamePassed;
                    }
                }
            }

            if (appInsightsInstrumentationKey != null && !string.IsNullOrEmpty(appInsightsInstrumentationKey.Value()))
            {
                string appInsightsInstrumentationKeyPassed = appInsightsInstrumentationKey.Value();
                if(creatorConfig.loggers != null && creatorConfig.loggers.Count > 0)
                {
                    if (!string.IsNullOrEmpty(appInsightNamePassed))
                    {
                        creatorConfig.loggers[0].name = appInsightNamePassed;
                    }
                    if(creatorConfig.loggers[0].credentials != null)
                    {
                        creatorConfig.loggers[0].credentials.instrumentationKey = appInsightsInstrumentationKeyPassed;
                    }
                }
            }
        }
    }
}
