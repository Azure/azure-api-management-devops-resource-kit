// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
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
            ProductApiTemplateCreator productAPITemplateCreator = new ProductApiTemplateCreator(new TemplateBuilder());
            CreatorParameters creatorConfig = new CreatorParameters() { Products = new List<ProductConfig>(), Apis = new List<ApiConfig>() };
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
            creatorConfig.Products.Add(product);
            ApiConfig api = new ApiConfig()
            {
                Name = "apiName",
                ApiVersion = "apiVersion",
                ApiVersionDescription = "apiVersionDescription",
                ApiVersionSetId = "apiVersionSetId",
                ApiRevision = "revision",
                ApiRevisionDescription = "revisionDescription",
                Suffix = "suffix",
                Products = "productName",
                SubscriptionRequired = true,
                AuthenticationSettings = new APITemplateAuthenticationSettings()
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
                OpenApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                Protocols = "https",
                IsCurrent = true,
                Type = "http"
            };
            creatorConfig.Apis.Add(api);

            // act
            Template productAPITemplate = productAPITemplateCreator.CreateProductAPITemplate(creatorConfig);
            ProductApiTemplateResource productAPITemplateResource = (ProductApiTemplateResource)productAPITemplate.Resources[0];

            // assert
            Assert.Equal($"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{product.Name}/{api.Name}')]", productAPITemplateResource.Name);
        }

        [Fact]
        public void ShouldCreateProductAPITemplateResourceFromValues()
        {

            // arrange
            ProductApiTemplateCreator productAPITemplateCreator = new ProductApiTemplateCreator(new TemplateBuilder());
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
            var productAPITemplateCreator = new ProductApiTemplateCreator(new TemplateBuilder());
            CreatorParameters creatorConfig = new CreatorParameters();
            var api = new ApiConfig()
            {
                Products = "1, 2, 3"
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
