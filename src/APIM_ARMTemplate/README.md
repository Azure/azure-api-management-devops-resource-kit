# Table of Contents

1. [ Creator ](#Creator)
    * [Deploy an APIM Instance](#creator1)
    * [Create the Config File](#creator2)
    * [Generate Templates Using the Create Command](#creator3)
    * [Deploying Linked Templates](#creator4)
    * [Deploying Unlinked Templates](#creator5)
2. [ Extractor ](#Extractor)

# Creator

<a name="creator1"></a>
## Deploy an APIM Instance

- Log into the Azure Portal
- Search for 'API Management services' in the bar at the top of the portal.
- Click 'Add' and fill out the necessary properties to create the instance

<a name="creator2"></a>
## Create the Config File

The utility requires one argument, --configFile, which points to a yaml file that links to policy and Open API Spec files and on which the entire process is dependent. The file contains a Creator Configuration object whose schema and related schemas are listed below:

### Schemas

#### Creator Configuration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| version               | string                | Yes                   | Configuration version.                            |
| apimServiceName       | string                | Yes                   | Name of APIM service to deploy resources into.  must match name of an APIM service deployed in the specified resource group. |
| apiVersionSet         | [APIVersionSetConfiguration](#APIVersionSetConfiguration) | No               | VersionSet configuration.                        |
| api                   | [APIConfiguration](#APIConfiguration)      | Yes                   | API configuration.                                |
| diagnostic            | [DiagnosticContractProperties](https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis/diagnostics#DiagnosticContractProperties) | No | Diagnostic configuration. |
| outputLocation        | string                | Yes                   | Local folder the utility will write templates to. |
| linked                | boolean               | No                    | Determines whether the utility will output linked or unlinked templates. |
| linkedTemplatesBaseUrl| string                | No                    | Remote location that stores linked templates. Required if 'linked' is set to true. |

#### APIVersionSetConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| id                    | string                | No                    | ID of the API Version Set.                        |
| displayName           | string                | No                    | Name of API Version Set.                          |
| description           | string                | No                    | Description of API Version Set.                  |
| versioningScheme      | enum                  | No                    | An value that determines where the API Version identifer will be located in a HTTP request. - Segment, Query, Header   |
| versionQueryName      | string                | No                    | Name of query parameter that indicates the API Version if versioningScheme is set to query.                             |
| versionHeaderName     | string                | No                    | Name of HTTP header parameter that indicates the API Version if versioningScheme is set to header.                            |

#### APIConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| name                  | string                | Yes                   | API identifier. Must be unique in the current API Management service instance.                                 |
| openApiSpec           | string                | Yes                   | Location of the Open API Spec file. Can be url or local file.                          |
| policy                | string                | No                    | Location of the API policy XML file. Can be url or local file.                          |
| suffix                | string                | Yes                    | Relative URL uniquely identifying this API and all of its resource paths within the API Management service instance. It is appended to the API endpoint base URL specified during the service instance creation to form a public URL for this API.                       |
| apiVersion            | string                | No                    | Indicates the Version identifier of the API if the API is versioned.                         |
| apiVersionDescription | string                | No                    | Description of the Api Version.                   |
| revision              | string                | No                    | Describes the Revision of the Api. If no value is provided, default revision 1 is created.                  |
| revisionDescription   | string                | No                    | Description of the Api Revision.                 |
| apiVersionSetId       | string                | No                    | A resource identifier for the related ApiVersionSet. Value must match the resource id on an existing version set and is irrelevant if the apiVersionSet property is supplied.       |
| operations            | Dictionary<string, [OperationPolicyConfiguration](#OperationPolicyConfiguration)> | No    | XML policies that will be applied to operations within the API. Keys must match the operationId property of one of the API's operations.                 |
| authenticationSettings| [AuthenticationSettingsContract](https://docs.microsoft.com/en-us/azure/templates/microsoft.apimanagement/2018-06-01-preview/service/apis#AuthenticationSettingsContract)                | No                    | Collection of authentication settings included into this API.                         |
| products              | string                | No                    | Comma separated list of existing products to associate the API with.                   |

#### OperationPolicyConfiguration

| Property              | Type                  | Required              | Value                                            |
|-----------------------|-----------------------|-----------------------|--------------------------------------------------|
| policy                | string                | No                    | Location of the operation policy XML file. Can be url or local file.      |

### Example File

The following is a full config.yml file with each property listed:

```
version: 0.0.1
apimServiceName: testapimlucas
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
  alwaysLog: allErrors
  loggerId: /loggers/applicationinsights,
  sampling:
    samplingType: fixed
    percentage: 50
  frontend: 
    request:
      headers:
        - Content-type
      body: 
        bytes: 512
    response: 
      headers:
        - Content-type
      body: 
        bytes: 512
  backend: 
    request:
      headers:
        - Content-type
      body: 
        bytes: 512
    response: 
      headers:
        - Content-type
      body: 
        bytes: 512
  enableHttpCorrelationHeaders: true
outputLocation: C:\Users\user1\Desktop\GeneratedTemplates
linked: true
linkedTemplatesBaseUrl : https://lucasyamlblob.blob.core.windows.net/yaml
```

<a name="creator3"></a>
## Generate Templates Using the Create Command

- Clone this repository and restore its packages
- Navigate to the azure-api-management-devops-example\src\APIM_ARMTemplate\apimtemplate directory
- ```dotnet run create --configFile CONFIG_YAML_FILE_LOCATION ```

The ```dotnet run create``` command will generate template and parameter files in the folder specified in the config file's outputLocation property.

<a name="creator4"></a>
## Deploying Linked Templates 

If the linked property in the config file is set to true, the utility will generate master template and parameters files, as well as a number of other templates the master template links to.

- Push each of the other templates to the location specified in the linkedTemplatesBaseUrl property in the config file (can be a GitHub repo, Azure blob storage container, etc)
- Navigate into the folder that contains the generated templates
- ```az group deployment create --resource-group YOUR_RESOURCE_GROUP --template-file ./master.template.json --parameters ./master.parameters.json```

<a name="creator5"></a>
## Deploying Unlinked Templates

If the linked property in the config file is set to false, the utility will generate two master templates and a parameters file, requiring two deployments.

- Navigate into the folder that contains the generated templates
- ```az group deployment create --resource-group YOUR_RESOURCE_GROUP --template-file ./master1.template.json --parameters ./master.parameters.json```
- ```az group deployment create --resource-group YOUR_RESOURCE_GROUP --template-file ./master2.template.json --parameters ./master.parameters.json```

# Extractor
