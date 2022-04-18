// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class ApiRevisionNotFoundException : Exception
    {
        public ApiRevisionNotFoundException(string message) : base(message)
        {
        }
    }
}
