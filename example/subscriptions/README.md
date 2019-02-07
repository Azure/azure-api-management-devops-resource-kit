# Subscriptions

This ARM template creates/modfies subscriptions for an existing API Management instance.

**What are subscriptions?**

When you publish APIs through API Management, it's easy and common to secure access to those APIs by using subscription keys. Developers who need to consume the published APIs must include a valid subscription key in HTTP requests when they make calls to those APIs. Otherwise, the calls are rejected immediately by the API Management gateway. They aren't forwarded to the back-end services.

To get a subscription key for accessing APIs, a subscription is required. A subscription is essentially a named container for a pair of subscription keys. Developers who need to consume the published APIs can get subscriptions. And they don't need approval from API publishers. API publishers can also create subscriptions directly for API consumers.

**Instructions:**

1. Clone this repo
2. Using a PowerShell , navigate to the `example/subscriptions` directory
3. Run following Azure PowerShell command to start the process:

```ps

	New-AzureRmResourceGroupDeployment -Name 'contoso-deployment' -ResourceGroupName <resource-group> -TemplateFile "subscriptions.*.template.json" -TemplateParameterFile "subscriptions.*.parameters.json"
```

**Note:**
1. Replace `<resource-group>` & parameter values that pertain to your environment in corresponding parameters file.
2. Currently, Subscriptions for all APIs or an individual API is available in the API Management Consumption tier only.

Refer: [Subscriptions in Azure API Management](https://docs.microsoft.com/en-us/azure/api-management/api-management-subscriptions) for more details.
