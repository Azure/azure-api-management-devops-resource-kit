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
        Template Build();

        TemplateBuilder GenerateTemplateWithApimServiceNameProperty();

        TemplateBuilder GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters);

        TemplateBuilder GenerateEmptyTemplate();

        TemplateBuilder AddPolicyProperties(ExtractorParameters extractorParameters);

        TemplateBuilder AddParameterizeServiceUrlProperty(ExtractorParameters extractorParameters);

        TemplateBuilder AddParameterizeApiLoggerIdProperty(ExtractorParameters extractorParameters);
    }
}
