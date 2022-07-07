// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public static async Task<IHttpClientFactory> GenerateMockedIHttpClientFactoryWithResponse(string fileLocation)
        {
            var jsonResponse = await GetFileContent(fileLocation);

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var response = new HttpResponseMessage
            {
                Content = new StringContent(jsonResponse)
            };

            httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return httpClientFactory.Object;
        }
    }
}
