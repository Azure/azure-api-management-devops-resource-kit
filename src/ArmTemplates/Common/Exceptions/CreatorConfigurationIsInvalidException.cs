// --------------------------------------------------------------------------
//  <copyright file="CreatorConfigurationIsInvalidException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions
{
    public class CreatorConfigurationIsInvalidException : Exception
    {
        public CreatorConfigurationIsInvalidException(string message) : base(message)
        {
        }
    }
}
