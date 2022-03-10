// --------------------------------------------------------------------------
//  <copyright file="ITemplateBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions
{
    public interface ITemplateBuilder
    {
        Template GenerateTemplateWithApimServiceNameProperty();

        Template GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters);

        Template GenerateEmptyTemplate();
    }
}
