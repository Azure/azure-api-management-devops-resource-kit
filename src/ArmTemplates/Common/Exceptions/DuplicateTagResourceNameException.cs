// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class DuplicateTagResourceNameException : Exception
    {
        public DuplicateTagResourceNameException(string message) : base(message)
        {
        }

        public DuplicateTagResourceNameException(string existingValue, string tagName, string resourceName): base (string.Format(ErrorMessages.DuplicateTagResourceNameErrorMessage, existingValue, tagName, resourceName))
        {
        }
    }
}
