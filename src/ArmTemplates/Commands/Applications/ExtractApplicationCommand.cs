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
        private ExtractorConfig _extractorConfig;

        public ExtractApplicationCommand() : base()
        {
        }

        protected override void SetupApplicationAndCommands()
        {
            Name = GlobalConstants.ExtractName;
            Description = GlobalConstants.ExtractDescription;

            var extractorConfigFilePathOption = Option("--extractorConfig <extractorConfig>", "Config file of the extractor", CommandOptionType.SingleValue);
            AddExtractorConfigPropertiesToCommandLineOptions();

            if (extractorConfigFilePathOption.HasValue())
            {
                var fileReader = new FileReader();
                _extractorConfig = fileReader.ConvertConfigJsonToExtractorConfig(extractorConfigFilePathOption.Value());
            }

            UpdateExtractorConfigFromAdditionalArguments();
        }

        protected override async Task<int> ExecuteCommand()
        {
            try
            {
                _extractorConfig.Validate();

                var extractorExecutor = new ExtractorExecutor(_extractorConfig);
                await extractorExecutor.ExecuteGenerationBasedOnExtractorConfiguration();

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

        private void AddExtractorConfigPropertiesToCommandLineOptions()
        {
            foreach (var propertyInfo in typeof(ExtractorConfig).GetProperties())
            {
                var description = Attribute.IsDefined(propertyInfo, typeof(DescriptionAttribute)) ? (Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) as DescriptionAttribute).Description : string.Empty;

                Option($"--{propertyInfo.Name} <{propertyInfo.Name}>", description, CommandOptionType.SingleValue);
            }
        }

        private void UpdateExtractorConfigFromAdditionalArguments()
        {
            var extractorConfigType = typeof(ExtractorConfig);
            foreach (var option in Options.Where(o => o.HasValue()))
            {
                extractorConfigType.GetProperty(option.LongName)?.SetValue(_extractorConfig, option.Value());
            }
        }
    }
}