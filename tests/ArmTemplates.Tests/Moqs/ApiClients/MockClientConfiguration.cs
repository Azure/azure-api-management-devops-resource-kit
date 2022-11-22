// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Net;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Moqs.ApiClients
{
    public class MockClientConfiguration
    {
        public string ResponseFileLocation { get; private set; }
        public HttpStatusCode ResponseStatusCode { get; private set; }

        public MockClientConfiguration(string responseFileLocation, HttpStatusCode statusCode = HttpStatusCode.OK) 
        {
            this.ResponseFileLocation = responseFileLocation;
            this.ResponseStatusCode = statusCode;
        }
    }
}
