// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Clients.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Moq;
using Moq.Protected;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockClientUtils
    {
        public const string ResourcesPath = "Resources";
        public const string ApiClientJsonResponsesPath = "ApiClientJsonResponses";
        public static AzureCliAuthenticator GetMockedAzureClient()
        {
            var mockedZureClientAuth = new Mock<AzureCliAuthenticator>(MockBehavior.Strict);
            mockedZureClientAuth.Setup(x => x.GetAccessToken()).ReturnsAsync(() => ("val1", "val2"));
            return mockedZureClientAuth.Object;
        }

        public static async Task<string> GetFileContent(string fileLocation)
        {
            var fileReader = new FileReader();
            return await fileReader.RetrieveFileContentsAsync(Path.Combine(ResourcesPath, fileLocation));
        }

        public static async Task<T> DeserializeFileContent<T>(string fileLocation)
        {
            var fileContent = await GetFileContent(fileLocation);
            return fileContent.Deserialize<T>();
        }

        public static async Task<IHttpClientFactory> GenerateMockedIHttpClientFactoryWithResponse(params MockClientConfiguration[] mockClientConfigurations)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            foreach (var mockClientConfiguration in mockClientConfigurations)
            {
                var jsonResponse = await GetFileContent(mockClientConfiguration.ResponseFileLocation);

                var response = new HttpResponseMessage
                {
                    Content = new StringContent(jsonResponse),
                    StatusCode = mockClientConfiguration.ResponseStatusCode
                };

                if (!string.IsNullOrEmpty(mockClientConfiguration.UrlPath))
                {
                    httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(p => p.RequestUri.ToString().EndsWith(mockClientConfiguration.UrlPath)), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);
                }
                else
                {
                    httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);
                }
            }

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return httpClientFactory.Object;
        }

        public static void MockAuthOfApiClient<T>(Mock<T> mockedClient) where T: ApiClientBase
        {
            mockedClient.Protected()
                .Setup<AzureCliAuthenticator>("Auth").Returns(GetMockedAzureClient());
        }
    }
}
