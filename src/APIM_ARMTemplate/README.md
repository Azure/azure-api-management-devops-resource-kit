[![Build Status](https://dev.azure.com/apim-devops/ARM-template-generator/_apis/build/status/Master%20-%20Quality%20Gate?branchName=master)](https://dev.azure.com/apim-devops/ARM-template-generator/_build/latest?definitionId=1?branchName=master)

# Azure API Management DevOps Example

This repository contains examples and tools to help you achieve API DevOps with Azure API Management.

## Background

Organizations today often have multiple deployment environments (e.g., development, QA, production) and use separate API Management (APIM) instances for each environment. In some cases, those APIM instances are shared by a number of API development teams, each responsible for one or more APIs. It becomes a challenge for organizations to automate deployment of APIs into a shared environment without interference between the teams and/or impact on API consumers.

In this repository, we propose an approach for the problem along with examples and tools to implement the solution.

## API DevOps with API Management

The proposed approach is shown in the below picture.

![alt](APIM-DevOps.png)

In this example, there are two deployment environments: _Development_ and _Production_, each environment has its own APIM instance. _API developers_ have access to the _Development_ instance and can use it for developing and testing their APIs. The _Production_ instance is managed by a designated team called the _API publishers_.

The key in this proposed solution is to keep all your APIM configurations in Azure Resource Manager (ARM) templates under your SCM systems. In this example, we use GIT. There is a _Publisher repository_ that contains all the configurations of the _Production_ instance. _API developers_ can fork and clone the _Publisher repository_ and then start working on their APIs in their own repository.

There are three types of ARM templates: the _Service template_ contains all the configurations related to the APIM instance (e.g., sku type, custom URL). The _Shared templates_ contain shared resources (e.g., groups, products). For each API, there is an _API template_ that contains all the configurations related to the API and its sub-resources (e.g., API definition, operations, policies).

Finally, The _Master template_ ties everything together by linking to all templates and deploy them in order. Therefore, if you want to deploy everything to an APIM instance, you can simply deploy the _Master template_. A beautiful thing is that each template can also be deployed individually. For example, if you just want to deploy a single API. 

_API developers_ normally work with Open API (aka., Swagger) files for defining their APIs. One challenge for them is authoring the _API templates_, which might be an error-prone task for people not familiar with ARM template formats. Therefore, we will create and open source an **ARM template generator** to help you generate _API templates_ based on Open API (aka., Swagger) files and policy files. Basically, the generator will extract your API and operation definitions from an Open API file and insert them into an ARM template in the proper format. It will also extract policies from an XML file and inline the policies into the template. With this tool, _API developers_ can continue focusing on the formats they are familiar with.

If you already have APIs published by APIM, or if you prefer using the Admin UX for configuring your APIs, we will also provide a **second ARM template generator** to help you generate _API templates_ by extracting configurations from an existing APIM instance. Likewise, this generator will also be open sourced. 

When _API developers_ finish develop and test an API, and have generated the _API template_ for it, they can submit a pull request to merge the changes back to the _Publisher repository_. _API publishers_ can validate the pull request to make sure the changes are safe and compliant to the corporate standards. Most of the validations can be automated as part of the CI/CD pipeline.

Once the changes are merged successfully, then _API publishers_ can deploy them to the _Production_ instance either on schedule or on request.

With this approach, it is easy to migrate changes from one environment to another. Also, since different API development teams will be working on different sets of API templates and files, it also prevents them from colliding with each other.

We have provided an [example](Example.md) on how to use this approach and deploy using the Azure CLI.

We realize our customers bring a wide range of engineering cultures and existing automation solutions. The approach proposed here is not meant to be a one-size-fits-all solution. That's the reason we created this repository and open sourced everything, so that you can extend and custom the solution.

If you have any questions or feedback, please raise issues in the repository or email us at apimgmt at microsoft dotcom. We also started an [FAQ page](./FAQ.md) to answer most common questions. 

## License

This project is licensed under [the MIT License](LICENSE)

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
