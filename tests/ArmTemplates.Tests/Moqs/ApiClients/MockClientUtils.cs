// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities;
using Moq;
using Moq.Protected;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockClientUtils
    {
        public static AzureCliAuthenticator GetMockedAzureClient()
        {
            var mockedZureClientAuth = new Mock<AzureCliAuthenticator>(MockBehavior.Strict);
            mockedZureClientAuth.Setup(x => x.GetAccessToken()).ReturnsAsync(() => ("val1", "val2"));
            return mockedZureClientAuth.Object;
        }

        public static async Task<IHttpClientFactory> GenerateMockedIHttpClientFactoryWithResponse(string jsonFileName)
        {
            var fileReader = new FileReader();
            var fileLocation = Path.Combine("Resources", "ApiClientJsonResponses", jsonFileName);
            var jsonResponse = await fileReader.RetrieveFileContentsAsync(fileLocation);

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var response = new HttpResponseMessage
            {
                Content = new StringContent(jsonResponse)
            };

            httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);
            var httpClient = new HttpClient(httpMessageHandlerMock.Object);

            var httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage.Content = new StringContent(jsonFileName);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return httpClientFactory.Object;
        }
    }
}
