// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications
{
    public class ExtractApplicationCommand : IConsoleAppCommand<ExtractorConsoleAppConfiguration, ExtractorParameters>
    {
        readonly ILogger<ExtractApplicationCommand> logger;
        readonly ExtractorExecutor extractorExecutor;

        public ExtractApplicationCommand(ILogger<ExtractApplicationCommand> logger, ExtractorExecutor extractorExecutor) : base()
        {
            this.logger = logger;
            this.extractorExecutor = extractorExecutor;
        }

        public async Task<ExtractorParameters> ParseInputConfigurationAsync(ExtractorConsoleAppConfiguration extractorConfiguration)
        {
            if (extractorConfiguration == null)
            {
                throw new ArgumentNullException(nameof(extractorConfiguration));
            }

            ExtractorParameters extractorParameters;

            if (!string.IsNullOrEmpty(extractorConfiguration.ExtractorConfig))
            {
                if (!File.Exists(extractorConfiguration.ExtractorConfig))
                {
                    throw new FileNotFoundException($"There was no extractor config found by this path: '{extractorConfiguration.ExtractorConfig}'");
                }

                using var streamReader = new StreamReader(extractorConfiguration.ExtractorConfig);
                var extractorConfigContents = await streamReader.ReadToEndAsync();
                var extractorFileConfiguration = extractorConfigContents.Deserialize<ExtractorConsoleAppConfiguration>();

                // here we have a configuration from file
                // plus another fields that maybe are overriding the file-configuration
                extractorParameters = new ExtractorParameters(extractorFileConfiguration);
                extractorParameters = extractorParameters.OverrideConfiguration(extractorConfiguration);
            }
            else
            {
                extractorParameters = new ExtractorParameters(extractorConfiguration);
            }

            extractorParameters.Validate();
            return extractorParameters;
        }

        public async Task ExecuteCommandAsync(ExtractorParameters parameters)
        {
            try
            {
                this.extractorExecutor.SetExtractorParameters(parameters);
                await this.extractorExecutor.ExecuteGenerationBasedOnConfiguration();

                this.logger.LogInformation("Templates written to output location");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occured: {ex.Message}");
                throw;
            }
        }
    }
}