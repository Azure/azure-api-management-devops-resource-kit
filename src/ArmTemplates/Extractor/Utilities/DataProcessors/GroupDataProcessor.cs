// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class GroupDataProcessor: IGroupDataProcessor
    {
        public IDictionary<string, string> OverrideRules { get; }

        public GroupDataProcessor()
        {
            this.OverrideRules = new Dictionary<string, string>()
            {
                { "Administrators", "administrators" },
                { "Developers", "developers" },
                { "Guests", "guests" }
            };
        }

        public void ProcessData(List<GroupTemplateResource> groupTemplates, ExtractorParameters extractorParameters) 
        {

            if (groupTemplates.IsNullOrEmpty())
            {
                return;
            }

            foreach (var groupTemplate in groupTemplates) 
            {
                groupTemplate.OriginalName = groupTemplate.Name;
                groupTemplate.NewName = groupTemplate.Name;

                if (extractorParameters.OverrideGroupGuids)
                {
                    this.OverrideName(groupTemplate);
                }
            }
        }

        public void OverrideName(GroupTemplateResource template)
        {
            if (this.OverrideRules.IsNullOrEmpty())
            {
                return;
            }

            if (this.OverrideRules.ContainsKey(template.Properties.DisplayName))
            {
                var newName = this.OverrideRules[template.Properties.DisplayName];
                
                if (!template.Name.Equals(newName)) {
                    template.OriginalName = template.Name;
                    template.NewName = newName;
                    template.Name = newName;
                }
            }
        }
    }
}
