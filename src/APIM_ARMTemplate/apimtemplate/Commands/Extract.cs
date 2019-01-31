using System;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract.Operation;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = Constants.ExtractName;
            this.Description = Constants.ExtractDescription;

            var apiManagementName = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue);
            var resourceGroupName = this.Option("--resourceGroup <resourceGroup>", "Resource Group name", CommandOptionType.SingleValue);

            this.HelpOption();

            this.OnExecute(async () =>
            {
                if (!apiManagementName.HasValue()) throw new Exception("Missing parameter <apimname>.");
                if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");

                string resourceGroup = resourceGroupName.Values[0].ToString();
                string apimname = apiManagementName.Values[0].ToString();
                Api api = new Api();
                string apis = api.GetAPIs(apimname, resourceGroup).Result;

                ExtractedAPI extractedAPI = JsonConvert.DeserializeObject<ExtractedAPI>(apis);
                Console.WriteLine("{0} API's found!", extractedAPI.value.Count.ToString());
                FileReader fileReader = new FileReader();
                
                for (int i = 0; i < extractedAPI.value.Count; i++)
                {
                    APIConfig apiConfig = new APIConfig();

                    CreatorConfig creatorConfig = new CreatorConfig
                    {
                        version = "1.0.0",
                        outputLocation = @"",
                        apimServiceName = apimname,
                        api = apiConfig
                    };
                    creatorConfig.api.openApiSpec = "{\"swagger\":\"2.0\",\"info\":{\"version\":\"1.0.0\",\"title\":\"Swagger Petstore\",\"description\":\"A sample API that uses a petstore as an example to demonstrate features in the swagger-2.0 specification\",\"termsOfService\":\"http://swagger.io/terms/\",\"contact\":{\"name\":\"Swagger API Team\",\"email\":\"apiteam@swagger.io\",\"url\":\"http://swagger.io\"},\"license\":{\"name\":\"Apache 2.0\",\"url\":\"https://www.apache.org/licenses/LICENSE-2.0.html\"}},\"host\":\"petstore.swagger.io\",\"basePath\":\"/api\",\"schemes\":[\"http\"],\"consumes\":[\"application/json\"],\"produces\":[\"application/json\"],\"paths\":{\"/pets\":{\"get\":{\"description\":\"Returns all pets from the system that the user has access to\\nNam sed condimentum est. Maecenas tempor sagittis sapien, nec rhoncus sem sagittis sit amet. Aenean at gravida augue, ac iaculis sem. Curabitur odio lorem, ornare eget elementum nec, cursus id lectus. Duis mi turpis, pulvinar ac eros ac, tincidunt varius justo. In hac habitasse platea dictumst. Integer at adipiscing ante, a sagittis ligula. Aenean pharetra tempor ante molestie imperdiet. Vivamus id aliquam diam. Cras quis velit non tortor eleifend sagittis. Praesent at enim pharetra urna volutpat venenatis eget eget mauris. In eleifend fermentum facilisis. Praesent enim enim, gravida ac sodales sed, placerat id erat. Suspendisse lacus dolor, consectetur non augue vel, vehicula interdum libero. Morbi euismod sagittis libero sed lacinia.\\n\\nSed tempus felis lobortis leo pulvinar rutrum. Nam mattis velit nisl, eu condimentum ligula luctus nec. Phasellus semper velit eget aliquet faucibus. In a mattis elit. Phasellus vel urna viverra, condimentum lorem id, rhoncus nibh. Ut pellentesque posuere elementum. Sed a varius odio. Morbi rhoncus ligula libero, vel eleifend nunc tristique vitae. Fusce et sem dui. Aenean nec scelerisque tortor. Fusce malesuada accumsan magna vel tempus. Quisque mollis felis eu dolor tristique, sit amet auctor felis gravida. Sed libero lorem, molestie sed nisl in, accumsan tempor nisi. Fusce sollicitudin massa ut lacinia mattis. Sed vel eleifend lorem. Pellentesque vitae felis pretium, pulvinar elit eu, euismod sapien.\\n\",\"operationId\":\"findPets\",\"parameters\":[{\"name\":\"tags\",\"in\":\"query\",\"description\":\"tags to filter by\",\"required\":false,\"type\":\"array\",\"collectionFormat\":\"csv\",\"items\":{\"type\":\"string\"}},{\"name\":\"limit\",\"in\":\"query\",\"description\":\"maximum number of results to return\",\"required\":false,\"type\":\"integer\",\"format\":\"int32\"}],\"responses\":{\"200\":{\"description\":\"pet response\",\"schema\":{\"type\":\"array\",\"items\":{\"$ref\":\"#/definitions/Pet\"}}},\"default\":{\"description\":\"unexpected error\",\"schema\":{\"$ref\":\"#/definitions/Error\"}}}},\"post\":{\"description\":\"Creates a new pet in the store.  Duplicates are allowed\",\"operationId\":\"addPet\",\"parameters\":[{\"name\":\"pet\",\"in\":\"body\",\"description\":\"Pet to add to the store\",\"required\":true,\"schema\":{\"$ref\":\"#/definitions/NewPet\"}}],\"responses\":{\"200\":{\"description\":\"pet response\",\"schema\":{\"$ref\":\"#/definitions/Pet\"}},\"default\":{\"description\":\"unexpected error\",\"schema\":{\"$ref\":\"#/definitions/Error\"}}}}},\"/pets/{id}\":{\"get\":{\"description\":\"Returns a user based on a single ID, if the user does not have access to the pet\",\"operationId\":\"find pet by id\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"description\":\"ID of pet to fetch\",\"required\":true,\"type\":\"integer\",\"format\":\"int64\"}],\"responses\":{\"200\":{\"description\":\"pet response\",\"schema\":{\"$ref\":\"#/definitions/Pet\"}},\"default\":{\"description\":\"unexpected error\",\"schema\":{\"$ref\":\"#/definitions/Error\"}}}},\"delete\":{\"description\":\"deletes a single pet based on the ID supplied\",\"operationId\":\"deletePet\",\"parameters\":[{\"name\":\"id\",\"in\":\"path\",\"description\":\"ID of pet to delete\",\"required\":true,\"type\":\"integer\",\"format\":\"int64\"}],\"responses\":{\"204\":{\"description\":\"pet deleted\"},\"default\":{\"description\":\"unexpected error\",\"schema\":{\"$ref\":\"#/definitions/Error\"}}}}}},\"definitions\":{\"Pet\":{\"type\":\"object\",\"allOf\":[{\"$ref\":\"#/definitions/NewPet\"},{\"required\":[\"id\"],\"properties\":{\"id\":{\"type\":\"integer\",\"format\":\"int64\"}}}]},\"NewPet\":{\"type\":\"object\",\"required\":[\"name\"],\"properties\":{\"name\":{\"type\":\"string\"},\"tag\":{\"type\":\"string\"}}},\"Error\":{\"type\":\"object\",\"required\":[\"code\",\"message\"],\"properties\":{\"code\":{\"type\":\"integer\",\"format\":\"int32\"},\"message\":{\"type\":\"string\"}}}}}";
                    creatorConfig.api.name = extractedAPI.value[i].name;
                    creatorConfig.api.apiVersion = extractedAPI.value[i].properties.apiVersion;
                    creatorConfig.api.apiVersionDescription = extractedAPI.value[i].properties.apiVersionDescription;
                    creatorConfig.api.suffix = extractedAPI.value[i].properties.path;     
                    creatorConfig.linked = true;

                    Dictionary<string, OperationsConfig> operationsDic = new Dictionary<string, OperationsConfig>();
                    //operation list
                    string operations = api.GetAPIOperations(apimname, resourceGroup, creatorConfig.api.name).Result;
                    var oOperations = JsonConvert.DeserializeObject<Operation.Operation>(operations);

                    //get operation policy                    
                    for (int o = 0; o < oOperations.value.Count; o++)
                    {
                        string operationName = oOperations.value[o].name;
                        string policies = api.GetOperationPolicy(apimname, resourceGroup, creatorConfig.api.name, operationName).Result;
                        var oPolicies = JsonConvert.DeserializeObject<Operation.Operation>(policies);
                        OperationsConfig operationsConfig = new OperationsConfig();

                        for (int oc = 0; oc < oPolicies.value.Count; oc++)
                        {
                            operationsConfig.policy = oPolicies.value[oc].properties.policyContent;
                            operationsDic.Add(operationName, operationsConfig);
                        }                        
                    }

                    creatorConfig.api.operations = operationsDic; 

                    if (extractedAPI.value[i].properties.apiVersionSetId != null)
                    {
                        string APIVersionSetFull = extractedAPI.value[i].properties.apiVersionSetId;
                        string APIVersionSetId = APIVersionSetFull.Substring(APIVersionSetFull.LastIndexOf('/') + 1);
                        APIVersionSetId = api.GetAPIVersionSet(apimname, resourceGroup, APIVersionSetId).Result;
                        APIVersionSetTemplateResource apiv = JsonConvert.DeserializeObject<APIVersionSetTemplateResource>(APIVersionSetId);

                        creatorConfig.apiVersionSet = apiv.properties;
                    }

                    FileWriter fileWriter = new FileWriter();
                    TemplateCreator templateCreator = new TemplateCreator();

                    APIVersionSetTemplateCreator apiVersionSetTemplateCreator = new APIVersionSetTemplateCreator(templateCreator);
                    ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
                    PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(creatorConfig);
                    APITemplateCreatorEx apiTemplateCreator = new APITemplateCreatorEx(templateCreator, policyTemplateCreator, productAPITemplateCreator);
                    MasterTemplateCreator masterTemplateCreator = new MasterTemplateCreator(templateCreator);

                    // create templates from provided configuration
                    CreatorFileNames creatorFileNames = fileWriter.GenerateCreatorFileNames();
                    Template apiVersionSetTemplate = creatorConfig.apiVersionSet != null ? apiVersionSetTemplateCreator.CreateAPIVersionSetTemplate(creatorConfig) : null;
                    Template initialAPITemplate = await apiTemplateCreator.CreateInitialAPITemplateAsync(creatorConfig);
                    Template subsequentAPITemplate = await apiTemplateCreator.CreateSubsequentAPITemplateAsync(creatorConfig);
                    if (creatorConfig.linked == true)
                    {
                        Template masterTemplate = masterTemplateCreator.CreateLinkedMasterTemplate(apiVersionSetTemplate, initialAPITemplate, subsequentAPITemplate, creatorFileNames);
                        Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);

                        // write templates to outputLocation
                        if (apiVersionSetTemplate != null)
                        {
                            fileWriter.WriteJSONToFile(apiVersionSetTemplate, string.Concat(creatorConfig.outputLocation, creatorFileNames.apiVersionSet));
                        }
                        fileWriter.WriteJSONToFile(initialAPITemplate, string.Concat(creatorConfig.outputLocation, creatorFileNames.initialAPI));
                        fileWriter.WriteJSONToFile(subsequentAPITemplate, string.Concat(creatorConfig.outputLocation, creatorFileNames.subsequentAPI));
                        fileWriter.WriteJSONToFile(masterTemplate, string.Concat(creatorConfig.outputLocation, creatorConfig.api.name, "-master.template.json"));
                        fileWriter.WriteJSONToFile(masterTemplateParameters, string.Concat(creatorConfig.outputLocation, creatorConfig.api.name, "-master.parameters.json"));
                    }
                    else
                    {
                        Template initialMasterTemplate = masterTemplateCreator.CreateInitialUnlinkedMasterTemplate(apiVersionSetTemplate, initialAPITemplate);
                        Template subsequentMasterTemplate = masterTemplateCreator.CreateSubsequentUnlinkedMasterTemplate(subsequentAPITemplate);
                        Template masterTemplateParameters = masterTemplateCreator.CreateMasterTemplateParameterValues(creatorConfig);
                        fileWriter.WriteJSONToFile(initialMasterTemplate, string.Concat(creatorConfig.outputLocation, creatorConfig.api.name , "-master1.template.json"));
                        fileWriter.WriteJSONToFile(subsequentMasterTemplate, string.Concat(creatorConfig.outputLocation, creatorConfig.api.name, "-master2.template.json"));
                        fileWriter.WriteJSONToFile(masterTemplateParameters, string.Concat(creatorConfig.outputLocation, creatorConfig.api.name, "-master.parameters.json"));
                    }                    
                }
                Console.WriteLine("Templates written to output location");
                Console.ReadKey();
            });
        }
    }
}