// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Builders;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Creator.TemplateCreatorFactories
{
    public class PolicyTemplateCreatorFactory
    {
        public static PolicyTemplateCreator GeneratePolicyTemplateCreator()
        {
            FileReader fileReader = new FileReader();
            PolicyTemplateCreator policyTemplateCreator = new PolicyTemplateCreator(fileReader, new TemplateBuilder());
            return policyTemplateCreator;
        }
    }
}
