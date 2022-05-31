﻿// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class EmptyResourceNameException : Exception
    {
        public EmptyResourceNameException(string resourceName) : base(string.Format(ErrorMessages.EmptyResourceNameAfterSanitizingErrorMessage, resourceName))
        {
        }
    }
}
