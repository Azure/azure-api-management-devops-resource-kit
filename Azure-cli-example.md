# Deploying Resource Manager templates using Azure CLI

The following instructions demonstrate how to deploy the contents of this example repository using the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).

#### Changing the `ApimServiceName` template parameter

Note that throughout the following steps, the ARM template parameter `ApimServiceName` will need to be unique. If you use the default of `contosoapim-dev` that is used in these templates you will get the following error:

```
Deployment failed. Correlation ID: {
  "code": "ServiceAlreadyExists",
  "message": "Api service already exists: contosoapim-dev",
  "details": null,
  "innerError": null
}
```

## Instructions

Login to your Azure subscription:

`az login`

Create a new resource group `apim-rg` that will be used to deploy an APIM instance:

`az group create -n apim-rg -l westeurope`

Deploy the _service template_ to host the APIM _instance_ (note this command can take several minutes to complete): 

`az group deployment create --resource-group apim-rg --template-file ./example/service.template.json --parameters ./example/service.parameters.json`

Deploy the `api-httpbin` _API template_ to create an API in the APIM instance:

`az group deployment create --resource-group apim-rg --template-file ./example/api-httpbin/api-httpbin.template.json --parameters ./example/api-httpbin/api-httpbin.parameters.json`

It is of interest the command to create the `api-httpbin` API uses an Open API (swagger) specification `api-httpbin.openapi.json` file when creating the API.