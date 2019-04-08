
# Create an API Revision and Switch it to Current

This sample creates an Http Bin Api with an Operation, Product and Policy. It creates a revision of the same API, which is not current. The sample then contains a template to switch the newly created revision to current, by creating a release of the Api.

## current
- api-httpbin.current.template.json

Execute this template to create a new `Http Bin` Api having a `GET` Operation and associated to the `Started` Product.

## rev2
- api-httpbin.rev2.template.json

Execute this template to create a copy of the `Http Bin` API having the same `GET` Operation and adds an additional `POST` operation.

## switch
- api-httpbin.switch.template.json

Execute this template to create a `beta` release and switch the `HttpBinApi;rev2` to be the current Api.

**To Execute the template using the script**

```Powershell
$resourceGroupName = "contosoOrganization"
$apimServiceName = "contoso"
New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile <templatefilename>.json -ApimServiceName $apimServiceName
```