# Integration: Application Insights

This ARM template integrates an existing API Management instance to new Application Insights instance.

**Instructions:**
1. Clone this repo
2. Using a command-line tool, navigate to the `example/diagostics` directory
3. Run following Azure CLI command to start the process:

	`az group deployment create --resource-group <resource-group> --template-file ./application-insights.template.json --parameters ApimServiceName=<apim-instance-name> --parameters @application-insights.parameters.json`

	Note: be sure to replace `<resource-group>`as well as `<apim-instance-name>` with their corresponding parameters and update the `application-insights.parameters.json` file with appropriate parameters that pertain to your environment. 
4. Once completed, your existing API Management instance should be integrated with a newly created Application Insights instance as described in: [How to integrate Azure API Management with Azure Application Insights](https://docs.microsoft.com/en-us/azure/api-management/api-management-howto-app-insights)