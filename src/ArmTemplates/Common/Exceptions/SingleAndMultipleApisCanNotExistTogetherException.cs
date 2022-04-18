// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class SingleAndMultipleApisCanNotExistTogetherException : Exception
    {
        public SingleAndMultipleApisCanNotExistTogetherException(string message) : base(message)
        {
        }
    }
}
