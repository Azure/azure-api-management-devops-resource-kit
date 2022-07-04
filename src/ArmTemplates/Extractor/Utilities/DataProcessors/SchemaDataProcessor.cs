// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Schemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class SchemaDataProcessor: ISchemaDataProcessor
    {
        public void ProcessData(List<SchemaTemplateResource> schemaTemplateResources, ExtractorParameters extractorParameters)
        {
            if (schemaTemplateResources.IsNullOrEmpty())
            {
                return;
            }

            foreach (var schemaTemplate in schemaTemplateResources)
            {
                schemaTemplate.OriginalName = schemaTemplate.Name;
            }
        }
    }
}
