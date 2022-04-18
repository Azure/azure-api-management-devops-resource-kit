// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorTests
{
    public class ProductAPITemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateProductAPIFromCreatorConfig()
        {
            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>(), apis = new List<APIConfig>() };
            ProductConfig product = new ProductConfig()
            {
                Name = "productName",
                DisplayName = "display name",
                Description = "description",
                Terms = "terms",
                SubscriptionRequired = true,
                ApprovalRequired = true,
                SubscriptionsLimit = 1,
                State = "state"
            };
            creatorConfig.products.Add(product);
            APIConfig api = new APIConfig()
            {
                name = "apiName",
                apiVersion = "apiVersion",
                apiVersionDescription = "apiVersionDescription",
                apiVersionSetId = "apiVersionSetId",
                apiRevision = "revision",
                apiRevisionDescription = "revisionDescription",
                suffix = "suffix",
                products = "productName",
                subscriptionRequired = true,
                authenticationSettings = new APITemplateAuthenticationSettings()
                {
                    OAuth2 = new APITemplateOAuth2()
                    {
                        AuthorizationServerId = "",
                        Scope = ""
                    },
                    Openid = new APITemplateOpenID()
                    {
                        OpenIdProviderId = "",
                        BearerTokenSendingMethods = new string[] { }
                    },
                    SubscriptionKeyRequired = true
                },
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                protocols = "https",
                isCurrent = true,
                type = "http"
            };
            creatorConfig.apis.Add(api);

            // act
            Template productAPITemplate = productAPITemplateCreator.CreateProductAPITemplate(creatorConfig);
            ProductApiTemplateResource productAPITemplateResource = (ProductApiTemplateResource)productAPITemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.Name}/{api.name}')]", productAPITemplateResource.Name);
        }

        [Fact]
        public void ShouldCreateProductAPITemplateResourceFromValues()
        {

            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator(new TemplateBuilder());
            string productId = "productId";
            string apiName = "apiName";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            ProductApiTemplateResource productAPITemplateResource = productAPITemplateCreator.CreateProductAPITemplateResource(productId, apiName, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productId}/{apiName}')]", productAPITemplateResource.Name);
            Assert.Equal(dependsOn, productAPITemplateResource.DependsOn);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfProductAPITemplateResourcesFromCreatorConfig()
        {
            // arrange
            var productAPITemplateCreator = new ProductAPITemplateCreator(new TemplateBuilder());
            CreatorConfig creatorConfig = new CreatorConfig();
            var api = new APIConfig()
            {
                products = "1, 2, 3"
            };
            int count = 3;
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            var productAPITemplateResources = productAPITemplateCreator.CreateProductAPITemplateResources(api, dependsOn);

            // assert
            Assert.Equal(count, productAPITemplateResources.Count);
        }
    }
}
