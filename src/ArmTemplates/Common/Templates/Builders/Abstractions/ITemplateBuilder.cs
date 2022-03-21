// --------------------------------------------------------------------------
//  <copyright file="ITemplateBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using System;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders.Abstractions
{
    public interface ITemplateBuilder
    {
        /// <summary>
        /// Builds non-generic template
        /// </summary>
        [Obsolete($"NonGeneric Template is obsolete. Please, use '{nameof(Build)}()' method for building template instead.")]
        Template Build();

        /// <summary>
        /// Builds generic template
        /// </summary>
        /// <typeparam name="TTemplateResources">type of resources of template, which has parameterless constructor</typeparam>
        public Template<TTemplateResources> Build<TTemplateResources>()
            where TTemplateResources : ITemplateResources, new();

        TemplateBuilder GenerateTemplateWithApimServiceNameProperty();

        TemplateBuilder GenerateTemplateWithPresetProperties(ExtractorParameters extractorParameters);

        TemplateBuilder GenerateEmptyTemplate();

        TemplateBuilder AddPolicyProperties(ExtractorParameters extractorParameters);

        TemplateBuilder AddParameterizeServiceUrlProperty(ExtractorParameters extractorParameters);

        TemplateBuilder AddParameterizeApiLoggerIdProperty(ExtractorParameters extractorParameters);
    }
}
