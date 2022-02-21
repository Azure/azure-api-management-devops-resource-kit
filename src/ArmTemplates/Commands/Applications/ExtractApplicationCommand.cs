using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Executors;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Applications
{
    public class ExtractApplicationCommand : CommandLineApplicationBase
    {
        ExtractorConsoleAppConfiguration extractorConfig;

        public ExtractApplicationCommand() : base()
        {
        }

        protected override void SetupApplicationAndCommands()
        {
            this.Name = GlobalConstants.ExtractName;
            this.Description = GlobalConstants.ExtractDescription;

            var extractorConfigFilePathOption = this.Option("--extractorConfig <extractorConfig>", "Config file of the extractor", CommandOptionType.SingleValue);
            this.AddExtractorConfigPropertiesToCommandLineOptions();

            if (extractorConfigFilePathOption.HasValue())
            {
                var fileReader = new FileReader();
                this.extractorConfig = fileReader.ConvertConfigJsonToExtractorConfig(extractorConfigFilePathOption.Value());
            }

            this.UpdateExtractorConfigFromAdditionalArguments();
        }

        protected override async Task<int> ExecuteCommand()
        {
            try
            {
                this.extractorConfig.Validate();

                var extractorExecutor = new ExtractorExecutor(this.extractorConfig);
                await extractorExecutor.ExecuteGenerationBasedOnConfiguration();

                Logger.LogInformation("Templates written to output location");
                Logger.LogInformation("Press any key to exit process:");
#if DEBUG
                Console.ReadKey();
#endif

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occured: {ex.Message}");
                throw;
            }
        }

        void AddExtractorConfigPropertiesToCommandLineOptions()
        {
            foreach (var propertyInfo in typeof(ExtractorConsoleAppConfiguration).GetProperties())
            {
                var description = Attribute.IsDefined(propertyInfo, typeof(DescriptionAttribute)) ? (Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) as DescriptionAttribute).Description : string.Empty;

                this.Option($"--{propertyInfo.Name} <{propertyInfo.Name}>", description, CommandOptionType.SingleValue);
            }
        }

        void UpdateExtractorConfigFromAdditionalArguments()
        {
            var extractorConfigType = typeof(ExtractorConsoleAppConfiguration);
            foreach (var option in this.Options.Where(o => o.HasValue()))
            {
                extractorConfigType.GetProperty(option.LongName)?.SetValue(this.extractorConfig, option.Value());
            }
        }
    }
}