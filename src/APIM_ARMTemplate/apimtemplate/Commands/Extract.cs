using System;
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

            this.HelpOption();

            this.OnExecute(async () =>
            {
                // convert config file to extractorConfig class
                FileReader fileReader = new FileReader();
                ExtractorConfig extractorConfig = fileReader.ConvertConfigJsonToExtractorConfig();

                try
                {
                    //validation check
                    ExtractorUtils.validationCheck(extractorConfig);

                    string singleApiName = extractorConfig.apiName;
                    bool splitAPIs = extractorConfig.splitAPIs != null && extractorConfig.splitAPIs.Equals("true");
                    bool hasVersionSetName = extractorConfig.apiVersionSetName != null;
                    bool hasSingleApi = singleApiName != null;
                    bool includeRevisions = extractorConfig.includeAllRevisions != null && extractorConfig.includeAllRevisions.Equals("true");

                    // start running extractor
                    Console.WriteLine("API Management Template");
                    Console.WriteLine("Connecting to {0} API Management Service on {1} Resource Group ...", extractorConfig.sourceApimName, extractorConfig.resourceGroup);

                    // initialize file helper classes
                    FileWriter fileWriter = new FileWriter();
                    FileNameGenerator fileNameGenerator = new FileNameGenerator();
                    FileNames fileNames = fileNameGenerator.GenerateFileNames(extractorConfig.sourceApimName);

                    if (splitAPIs)
                    {
                        // create split api templates for all apis in the sourceApim
                        await ExtractorUtils.GenerateSplitAPITemplates(extractorConfig, fileNameGenerator, fileWriter, fileNames);
                    }
                    else if (hasVersionSetName)
                    {
                        // create split api templates and aggregated api templates for this apiversionset
                        await ExtractorUtils.GenerateAPIVersionSetTemplates(extractorConfig, fileNameGenerator, fileNames, fileWriter);
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
    }
}
