# Instructions

## Deploy an APIM Instance

- Log into the Azure Portal
- Search for 'API Management services' in the bar at the top of the portal.
- Click 'Add' and fill out the necessary properties to create the instance

## Create the Config File

The utility requires one argument, --configFile, which points to a yaml file that links to policy and Open API Spec files and on which the entire process is dependent. The following is a full config.yml file with each property listed:

```
version: 0.0.1   # Required
apimServiceName: testapimlucas   # Required, must match name of an apim service deployed in the specified resource group
apiVersionSet:   # Optional
    id: myAPIVersionSetID
    displayName: myAPIVersionSet
    description: a description
    versioningScheme: Query
    versionQueryName: versionQuery
    versionHeaderName: versionHeader
api:   # Required
  name: myAPI   # Required
  openApiSpec: ./swaggerPetstore.json   # Required, can be url or local file
  policy: ./apiPolicyHeaders.xml   # Optional, can be url or local file
  suffix: conf   # Required
  apiVersion: v1   # Optional
  apiVersionDescription: My first version   # Optional
  revision: 1   # Optional
  revisionDescription: My first revision   # Optional
  #apiVersionSetId: myID    # Optional, must match the resource id on an existing version set. Irrelevant if a version set is created in the config file.
  operations:   # Optional
    addPet: # Must match the operationId property of a path's operations
      policy: ./operationRateLimit.xml   # Optional, can be url or local file
    deletePet:  # Must match the operationId property of a path's operations
      policy: ./operationRateLimit.xml   # Optional, can be url or local file
  authenticationSettings:   # Optional
    subscriptionKeyRequired: false
    oAuth2:
        authorizationServerId: serverId
        scope: scope
  products: starter, platinum    # Optional, adds api to the specified products
diagnostic:   # Optional
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
outputLocation: C:\Users\user1\Desktop\GeneratedTemplates   # Required, folder the utility will write the templates to
linked: true   # Optional
linkedTemplatesBaseUrl : https://lucasyamlblob.blob.core.windows.net/yaml   # Required if 'linked' property is set to true
```


## Generate Templates Using the Create Command

- Clone this repository and restore its packages
- Navigate to the azure-api-management-devops-example\src\APIM_ARMTemplate\apimtemplate directory
- ```dotnet run create --configFile CONFIG_YAML_FILE_LOCATION ```

The ```dotnet run create``` command will generate template and parameter files in the folder specified in the config file's outputLocation property.

## Deploying Linked Templates 

If the linked property in the config file is set to true, the utility will generate master template and parameters files, as well as a number of other templates the master template links to.

- Push each of the other templates to the location specified in the linkedTemplatesBaseUrl property in the config file (can be a GitHub repo, Azure blob storage container, etc)
- Navigate into the folder that contains the generated templates
- ```az group deployment create --resource-group YOUR_RESOURCE_GROUP --template-file ./master.template.json --parameters ./master.parameters.json```

## Deploying Unlinked Templates

If the linked property in the config file is set to false, the utility will generate two master templates and a parameters file, requiring two deployments.

- Navigate into the folder that contains the generated templates
- ```az group deployment create --resource-group YOUR_RESOURCE_GROUP --template-file ./master1.template.json --parameters ./master.parameters.json```
- ```az group deployment create --resource-group YOUR_RESOURCE_GROUP --template-file ./master2.template.json --parameters ./master.parameters.json```