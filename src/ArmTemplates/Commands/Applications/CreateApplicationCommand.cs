// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications
{
    public class CreateApplicationCommand : IConsoleAppCommand<CreateConsoleAppConfiguration, CreatorParameters>
    {
        readonly ILogger<CreateApplicationCommand> logger;
        readonly CreatorExecutor creatorExecutor;
        readonly FileReader fileReader;

        public CreateApplicationCommand(
            ILogger<CreateApplicationCommand> logger, 
            CreatorExecutor creatorExecutor,
            FileReader fileReader)
        {
            this.logger = logger;
            this.creatorExecutor = creatorExecutor;
            this.fileReader = fileReader;
        }

        public async Task<CreatorParameters> ParseInputConfigurationAsync(CreateConsoleAppConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(CreateConsoleAppConfiguration));
            }

            if (string.IsNullOrEmpty(configuration.ConfigFile))
            {
                throw new ArgumentNullException(nameof(configuration.ConfigFile));
            }

            var creatorParameters = await this.fileReader.ConvertConfigYAMLToCreatorConfigAsync(configuration.ConfigFile);
            creatorParameters.OverrideParameters(configuration, this.fileReader);

            var creatorConfigurationValidator = new CreatorConfigurationValidator(creatorParameters);
            creatorConfigurationValidator.ValidateCreatorConfig();

            creatorParameters.GenerateFileNames();
            return creatorParameters;
        }

        public async Task ExecuteCommandAsync(CreatorParameters creatorParameters)
        {
            try
            {
                this.creatorExecutor.SetCreatorParameters(creatorParameters);
                await this.creatorExecutor.ExecuteGenerationBasedOnConfiguration();

                this.logger.LogInformation("Templates written to output location");
                this.logger.LogInformation("Creator finished execution!");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occured: {ex.Message}");
                throw;
            }
        }
    }
}
