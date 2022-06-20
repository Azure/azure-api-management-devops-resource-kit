// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.API.Models
{
    /// <summary>
    /// ClientHttpMethod represents HttpMethod type. The class is stored in a separate enum-type to not include System.Web.Mvc to the application.
    /// </summary>
    public enum ClientHttpMethod
    {
        GET,
        POST
    }
}
