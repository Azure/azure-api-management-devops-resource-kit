// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators.Abstractions;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories
{
    public class APITemplateCreatorFactory
    {
        public static IApiTemplateCreator GenerateAPITemplateCreator()
        {
            FileReader fileReader = new FileReader();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader, new TemplateBuilder());
            ProductApiTemplateCreator productAPITemplateCreator = new ProductApiTemplateCreator(new TemplateBuilder());
            DiagnosticTemplateCreator diagnosticTemplateCreator = new DiagnosticTemplateCreator();
            ReleaseTemplateCreator releaseTemplateCreator = new ReleaseTemplateCreator();
            TagApiTemplateCreator tagAPITemplateCreator = new TagApiTemplateCreator();
            ApiTemplateCreator apiTemplateCreator = new ApiTemplateCreator(fileReader, policyTemplateCreator, productAPITemplateCreator, tagAPITemplateCreator, diagnosticTemplateCreator, releaseTemplateCreator, new TemplateBuilder());
            return apiTemplateCreator;
        }
    }
}
