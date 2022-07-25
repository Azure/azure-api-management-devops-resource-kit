// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class ApiDataProcessor : IApiDataProcessor
    {

        public void ProcessData(List<ApiTemplateResource> apis)
        {
            if (apis.IsNullOrEmpty())
            {
                return;
            }

            foreach (var api in apis)
            {
                this.ProcessSingleData(api);
            }
        }

        public void ProcessSingleData(ApiTemplateResource api)
        {
            api.OriginalName = api.Name;
            api.Properties.LocalIsCurrent = api.Properties.IsCurrent;
            api.Properties.IsCurrent = null;

            if (api.Properties.LocalIsCurrent == true && !string.IsNullOrEmpty(api.Properties.ApiRevision))
            {
                api.ApiNameWithRevision = $"{api.Name};rev={api.Properties.ApiRevision}";
                if (!api.Name.Contains($";rev={api.Properties.ApiRevision}"))
                {
                    api.Name = $"{api.Name};rev={api.Properties.ApiRevision}";
                }
            }
        }
    }
}
