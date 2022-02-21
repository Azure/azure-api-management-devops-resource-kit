using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications
{
    public class ExtractApplicationCommand : CommandLineApplicationBase, ICommand
    {
        readonly ILogger<ExtractApplicationCommand> logger;
        readonly ExtractorExecutor extractorExecutor;

        ExtractorConsoleAppConfiguration extractorConfig;

        public ExtractApplicationCommand(ILogger<ExtractApplicationCommand> logger, ExtractorExecutor extractorExecutor) : base()
        {
            this.extractorExecutor = extractorExecutor;
        }

        protected override void SetupApplicationAndCommands()
        {
            this.Name = GlobalConstants.ExtractName;
            this.Description = GlobalConstants.ExtractDescription;

            this.AddExtractorConfigPropertiesToCommandLineOptions();
        }

        protected override async Task<int> ExecuteCommand()
        {
            try
            {
                this.FillInOptionsToExtractorConfig();
                this.extractorConfig.Validate();

                this.extractorExecutor.SetExtractorParameters(this.extractorConfig);
                await this.extractorExecutor.ExecuteGenerationBasedOnConfiguration();

                this.logger.LogInformation("Templates written to output location");
                this.logger.LogInformation("Press any key to exit process:");
#if DEBUG
                Console.ReadKey();
#endif

                return 1;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occured: {ex.Message}");
                throw;
            }
        }

        void FillInOptionsToExtractorConfig()
        {
            var extractorConfigOption = this.Options.FirstOrDefault(option => option.LongName == nameof(ExtractorConsoleAppConfiguration.ExtractorConfig).ToLowerFirstChar());
            if (extractorConfigOption?.HasValue() == true)
            {
                var fileReader = new FileReader();
                this.extractorConfig = fileReader.ConvertConfigJsonToExtractorConfig(extractorConfigOption.Value());
            }

            this.UpdateExtractorConfigFromAdditionalArguments();
        }

        void AddExtractorConfigPropertiesToCommandLineOptions()
        {
            foreach (var propertyInfo in typeof(ExtractorConsoleAppConfiguration).GetProperties())
            {
                var description = Attribute.IsDefined(propertyInfo, typeof(DescriptionAttribute)) ? (Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) as DescriptionAttribute).Description : string.Empty;
                var lowerFirstCharName = propertyInfo.Name.ToLowerFirstChar();

                this.Option($"--{lowerFirstCharName} <{lowerFirstCharName}>", description, CommandOptionType.SingleValue);
            }
        }

        void UpdateExtractorConfigFromAdditionalArguments()
        {
            var extractorConfigType = typeof(ExtractorConsoleAppConfiguration);
            foreach (var option in this.Options.Where(o => o.HasValue()))
            {
                extractorConfigType.GetProperty(option.LongName.ToUpperFirstChar())?.SetValue(this.extractorConfig, option.Value());
            }
        }
    }
}