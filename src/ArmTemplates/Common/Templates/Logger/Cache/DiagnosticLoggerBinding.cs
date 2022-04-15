// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Logger.Cache
{
    public class DiagnosticLoggerBinding
    {
        public string DiagnosticName { get; set; }

        public string LoggerId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DiagnosticLoggerBinding binding &&
                   this.DiagnosticName == binding.DiagnosticName &&
                   this.LoggerId == binding.LoggerId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.DiagnosticName, this.LoggerId);
        }
    }
}
