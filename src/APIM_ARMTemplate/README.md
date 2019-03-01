# Table of Contents

1. [ Creator ](#Creator)
    * [Create the Config File](#creator1)
    * [Running the Creator](#creator2)
2. [ Extractor ](#Extractor)
    * [Running the Extractor](#extrator1)

# Creator

This utility creates [Resource Manager templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authoring-templates) for an API based on the [OpenAPI Specification](https://github.com/OAI/OpenAPI-Specification) of the API. Optionaly, you can provide policies you wish to apply to the API and its operations in seperate files.

<a name="creator1"></a>

## Create the Config File

The utility requires one argument, --configFile, which points to a yaml file that links to policy and Open API Spec files and on which the entire process is dependent. The file contains a Creator Configuration object whose schema and related schemas are listed below:

### Schemas

#### Creator Configuration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| version               | string                | Yes                   | Configuration version.                            |
| apimServiceName       | string                | Yes                   | Name of APIM service to deploy resources into.    |
| apiVersionSet         | [APIVersionSetConfiguration](#APIVersionSetConfiguration) | No               | VersionSet configuration.                        |
| api                   | [APIConfiguration](#APIConfiguration)      | Yes                   | API configuration.                                |
| outputLocation        | string                | Yes                   | Local folder the utility will write templates to. |
| linked                | boolean               | No                    | Determines whether the utility should create a master template that links to all generated templates. |
| linkedTemplatesBaseUrl| string                | No                    | Location that stores linked templates. Required if 'linked' is set to true. |

#### APIVersionSetConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| id                    | string                | No                    | ID of the API Version Set.                        |
| displayName           | string                | Yes                    | Name of API Version Set.                          |
| description           | string                | No                    | Description of API Version Set.                  |
| versioningScheme      | enum                  | Yes                    | A value that determines where the API Version identifer will be located in a HTTP request. - Segment, Query, Header   |
| versionQueryName      | string                | No                    | Name of query parameter that indicates the API Version if versioningScheme is set to query.                             |
| versionHeaderName     | string                | No                    | Name of HTTP header parameter that indicates the API Version if versioningScheme is set to header.                            |

#### APIConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| name                  | string                | Yes                   | API identifier. Must be unique in the current API Management service instance.                                 |
| openApiSpec           | string                | Yes                   | Location of the Open API Spec file. Can be url or local file.                          |
| policy                | string                | No                    | Location of the API policy XML file. Can be url or local file.                          |
| suffix                | string                | Yes                    | Relative URL uniquely identifying this API and all of its resource paths within the API Management service instance. It is appended to the API endpoint base URL specified during the service instance creation to form a public URL for this API.                       |
| subscriptionRequired  | boolean               | No                    | Specifies whether an API or Product subscription is required for accessing the API.                         |
| apiVersion            | string                | No                    | Indicates the Version identifier of the API if the API is versioned.                         |
| apiVersionDescription | string                | No                    | Description of the Api Version.                   |
| revision              | string                | No                    | Describes the Revision of the Api. If no value is provided, default revision 1 is created.                  |
| revisionDescription   | string                | No                    | Description of the Api Revision.                 |
| apiVersionSetId       | string                | No                    | A resource identifier for the related ApiVersionSet. Value must match the resource id on an existing version set and is irrelevant if the apiVersionSet property is supplied.       |
| operations            | Dictionary<string, [APIOperationPolicyConfiguration](#APIOperationPolicyConfiguration)> | No    | XML policies that will be applied to operations within the API. Keys must match the operationId property of one of the API's operations.                 |
| authenticationSettings| [AuthenticationSettingsContract](https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis#AuthenticationSettingsContract)                | No                    | Collection of authentication settings included into this API.                         |
| products              | string                | No                    | Comma separated list of existing products to associate the API with.                   |
| diagnostic            | [APIDiagnosticConfiguration](#APIDiagnosticConfiguration) | No | Diagnostic configuration. |

#### APIOperationPolicyConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| policy                | string                | Yes                    | Location of the operation policy XML file. Can be url or local file.      |

#### APIDiagnosticConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| name                  | enum                | No                    | Name of API Diagnostic - azureEventHub or applicationInsights       |
| alwaysLog             | enum                | No                    | Specifies for what type of messages sampling settings should not apply. - allErrors       |
| loggerId              | string              | Yes                    | Resource Id of an existing target logger.       |
| sampling              | object                | No                    | Sampling settings for Diagnostic. - [SamplingSettings object](https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis/diagnostics#SamplingSettings)       |
| frontend              | object                | No                    | Diagnostic settings for incoming/outgoing HTTP messages to the Gateway. - [PipelineDiagnosticSettings object](https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis/diagnostics#PipelineDiagnosticSettings)       |
| backend                  | object                | No                    | Diagnostic settings for incoming/outgoing HTTP messages to the Backend - [PipelineDiagnosticSettings object](https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis/diagnostics#PipelineDiagnosticSettings)       |
| enableHttpCorrelationHeaders | boolean                | No                    | Whether to process Correlation Headers coming to Api Management Service. Only applicable to Application Insights diagnostics. Default is true.      |

### Sample Config File

The following is a full config.yml file with each property listed:

```
version: 0.0.1
apimServiceName: MyAPIMService
apiVersionSet:
    id: myAPIVersionSetID
    displayName: myAPIVersionSet
    description: a description
    versioningScheme: Query
    versionQueryName: versionQuery
    versionHeaderName: versionHeader
api:
  name: myAPI 
  openApiSpec: ./swaggerPetstore.json
  policy: ./apiPolicyHeaders.xml
  suffix: conf
  subscriptionRequired: true
  apiVersion: v1
  apiVersionDescription: My first version
  revision: 1
  revisionDescription: My first revision
  #apiVersionSetId: myID
  operations:
    addPet:
      policy:
    deletePet:
      policy:
  authenticationSettings:
    subscriptionKeyRequired: false
    oAuth2:
        authorizationServerId: serverId
        scope: scope
  products: starter, platinum
  diagnostic:
    name: applicationinsights
    alwaysLog: allErrors
    loggerId: /subscriptions/24de26c5-cf48-4954-8400-013fe61042wd/resourceGroups/MyResourceGroup/providers/Microsoft.ApiManagement/service/MyAPIMService/loggers/myappinsights,
    sampling:
      samplingType: fixed
      percentage: 50
    frontend: 
      request:
        headers:
        body: 
          bytes: 512
      response: 
        headers:
        body: 
          bytes: 512
    backend: 
      request:
        headers:
        body: 
          bytes: 512
      response: 
        headers:
        body: 
          bytes: 512
    enableHttpCorrelationHeaders: true
outputLocation: C:\Users\user1\Desktop\GeneratedTemplates
linked: true
linkedTemplatesBaseUrl : https://mystorageaccount.blob.core.windows.net/mycontainer
```

<a name="creator2"></a>

## Running the Creator
Below are the steps to run the Creator from the source code:

- Clone this repository and restore its packages using ```dotnet restore```
- Navigate to {path_to_folder}/src/APIM_ARMTemplate/apimtemplate directory
- Run the following command:
```dotnet run create --configFile CONFIG_YAML_FILE_LOCATION ```

You can also run it directly from the [releases](https://github.com/Azure/azure-api-management-devops-resource-kit/releases).

# Extractor

This utility generates [Resource Manager templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authoring-templates) by extracing existing configurations of one or more APIs in an API Management instance. 

<a name="prerequisite"></a>

## Prerequisite

To be able to run the Extractor, you would first need to [install the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).

<a name="extractor1"></a>

## Running the Extractor
Below are the steps to run the Extractor from the source code:
- Clone this repository and restore its packages using ```dotnet restore```
- Navigate to {path_to_folder}/src/APIM_ARMTemplate/apimtemplate directory
- Make sure you have signed in using Azure CLI and have switched to the subscription containing the API Management instance from which the configurations will be extracted. 
```
az login
az account set --subscription <subscription_id>
```
- Run the Extractor with the following command: 
```
dotnet run extract --name <name_of_the_APIM_instance> --resourceGroup <name_of_resource_group> --fileFolder <path_to_folder>
```

You can also run it directly from the [releases](https://github.com/Azure/azure-api-management-devops-resource-kit/releases).
