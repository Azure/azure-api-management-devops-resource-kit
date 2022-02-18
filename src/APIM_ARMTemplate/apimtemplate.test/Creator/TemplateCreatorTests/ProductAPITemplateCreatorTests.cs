using System.Collections.Generic;
using Xunit;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Test
{
    public class ProductAPITemplateCreatorTests
    {
        [Fact]
        public void ShouldCreateProductAPIFromCreatorConfig()
        {
            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig() { products = new List<ProductConfig>(), apis = new List<APIConfig>() };
            ProductConfig product = new ProductConfig()
            {
                name = "productName",
                displayName = "display name",
                description = "description",
                terms = "terms",
                subscriptionRequired = true,
                approvalRequired = true,
                subscriptionsLimit = 1,
                state = "state"
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
                    oAuth2 = new APITemplateOAuth2()
                    {
                        authorizationServerId = "",
                        scope = ""
                    },
                    openid = new APITemplateOpenID()
                    {
                        openidProviderId = "",
                        bearerTokenSendingMethods = new string[] { }
                    },
                    subscriptionKeyRequired = true
                },
                openApiSpec = "https://petstore.swagger.io/v2/swagger.json",
                protocols = "https",
                isCurrent = true,
                type = "http"
            };
            creatorConfig.apis.Add(api);

            // act
            Template productAPITemplate = productAPITemplateCreator.CreateProductAPITemplate(creatorConfig);
            ProductAPITemplateResource productAPITemplateResource = (ProductAPITemplateResource)productAPITemplate.resources[0];

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{product.name}/{api.name}')]", productAPITemplateResource.name);
        }

        [Fact]
        public void ShouldCreateProductAPITemplateResourceFromValues()
        {

            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            string productId = "productId";
            string apiName = "apiName";
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            ProductAPITemplateResource productAPITemplateResource = productAPITemplateCreator.CreateProductAPITemplateResource(productId, apiName, dependsOn);

            // assert
            Assert.Equal($"[concat(parameters('ApimServiceName'), '/{productId}/{apiName}')]", productAPITemplateResource.name);
            Assert.Equal(dependsOn, productAPITemplateResource.dependsOn);
        }

        [Fact]
        public void ShouldCreateCorrectNumberOfProductAPITemplateResourcesFromCreatorConfig()
        {
            // arrange
            ProductAPITemplateCreator productAPITemplateCreator = new ProductAPITemplateCreator();
            CreatorConfig creatorConfig = new CreatorConfig();
            APIConfig api = new APIConfig()
            {
                products = "1, 2, 3"
            };
            int count = 3;
            string[] dependsOn = new string[] { "dependsOn" };

            // act
            List<ProductAPITemplateResource> productAPITemplateResources = productAPITemplateCreator.CreateProductAPITemplateResources(api, dependsOn);

            // assert
            Assert.Equal(count, productAPITemplateResources.Count);
        }
    }
}
