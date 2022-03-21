// --------------------------------------------------------------------------
//  <copyright file="ServiceApiNotFoundException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class ServiceApiNotFoundException : Exception
    {
        public ServiceApiNotFoundException(string message) : base(message)
        {
        }
    }
}
