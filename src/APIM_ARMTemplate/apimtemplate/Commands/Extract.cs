using System;
using System.ComponentModel;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = GlobalConstants.ExtractName;
            this.Description = GlobalConstants.ExtractDescription;
            var extractorConfigFilePathOption = this.Option("--extractorConfig <extractorConfig>", "Config file of the extractor", CommandOptionType.SingleValue);
            AddExtractorConfigPropertiesToCommandLineOptions();

            this.HelpOption();

            this.OnExecute(async () =>
            {
                ExtractorConfig extractorConfig = new ExtractorConfig();

                if (extractorConfigFilePathOption.HasValue())
                {
                    var fileReader = new FileReader();
                    extractorConfig = fileReader.ConvertConfigJsonToExtractorConfig(extractorConfigFilePathOption.Value());
                }

                UpdateExtractorConfigFromAdditionalArguments(extractorConfig);

                try
                {
                    extractorConfig.Validate();

                    string singleApiName = extractorConfig.apiName;
                    bool splitAPIs = extractorConfig.splitAPIs != null && extractorConfig.splitAPIs.Equals("true");
                    bool hasVersionSetName = extractorConfig.apiVersionSetName != null;
                    bool hasSingleApi = singleApiName != null;
                    bool includeRevisions = extractorConfig.includeAllRevisions != null && extractorConfig.includeAllRevisions.Equals("true");
                    bool hasMultipleAPIs = extractorConfig.multipleAPIs != null;
                    EntityExtractor.baseUrl = (extractorConfig.serviceBaseUrl == null) ? EntityExtractor.baseUrl : extractorConfig.serviceBaseUrl;

                    // start running extractor
                    Console.WriteLine("API Management Template");
                    Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", extractorConfig.sourceApimName, extractorConfig.resourceGroup);

                    // initialize file helper classes
                    FileWriter fileWriter = new FileWriter();
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    FileNames fileNames = extractorConfig.baseFileName == null ?  fileNameGenerator.GenerateFileNames(extractorConfig.sourceApimName) : fileNameGenerator.GenerateFileNames(extractorConfig.baseFileName);

                    if (splitAPIs)
                    {
                        // create split api templates for all apis in the sourceApim
                        await ExtractorUtils.GenerateSplitAPITemplates(extractorConfig, fileNameGenerator, fileWriter, fileNames);
                        await ExtractorUtils.GenerateTemplates(new Extractor(extractorConfig), null, null, fileNameGenerator, fileNames, fileWriter, null);
                    }
                    else if (hasVersionSetName)
                    {
                        // create split api templates and aggregated api templates for this apiversionset
                        await ExtractorUtils.GenerateAPIVersionSetTemplates(extractorConfig, fileNameGenerator, fileNames, fileWriter);
                    }
                    else if (hasMultipleAPIs)
                    {
                        // generate templates for multiple APIs
                        await ExtractorUtils.GenerateMultipleAPIsTemplates(extractorConfig, fileNameGenerator, fileWriter, fileNames);
                    }
                    else if (hasSingleApi && includeRevisions)
                    {
                        // handle singel API include Revision extraction
                        await ExtractorUtils.GenerateSingleAPIWithRevisionsTemplates(extractorConfig, singleApiName, fileNameGenerator, fileWriter, fileNames);
                    }
                    else
                    {
                        // create single api template or create aggregated api templates for all apis within the sourceApim
                        if (hasSingleApi)
                        {
                            Console.WriteLine("Executing extraction for {0} API ...", singleApiName);
                        }
                        else
                        {
                            Console.WriteLine("Executing full extraction ...");
                        }
                        await ExtractorUtils.GenerateTemplates(new Extractor(extractorConfig), singleApiName, null, fileNameGenerator, fileNames, fileWriter, null);
                    }
                    Console.WriteLine("Templates written to output location");
                    Console.WriteLine("Press any key to exit process:");
#if DEBUG
                    Console.ReadKey();
#endif
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured: " + ex.Message);
                    throw;
                }
            });
        }

        private void AddExtractorConfigPropertiesToCommandLineOptions()
        {
            foreach (var propertyInfo in typeof(ExtractorConfig).GetProperties())
            {
                var description = Attribute.IsDefined(propertyInfo, typeof(DescriptionAttribute)) ? (Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) as DescriptionAttribute).Description : string.Empty;

                this.Option($"--{propertyInfo.Name} <{propertyInfo.Name}>", description, CommandOptionType.SingleValue);
            }
        }

        private void UpdateExtractorConfigFromAdditionalArguments(ExtractorConfig extractorConfig)
        {
            var extractorConfigType = typeof(ExtractorConfig);
            foreach (var option in this.Options.Where(o => o.HasValue()))
            {
                extractorConfigType.GetProperty(option.LongName)?.SetValue(extractorConfig, option.Value());
            }
        }
    }
}