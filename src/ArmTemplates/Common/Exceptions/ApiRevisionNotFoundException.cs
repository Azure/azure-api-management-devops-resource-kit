// --------------------------------------------------------------------------
//  <copyright file="ApiRevisionNotFound.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
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
