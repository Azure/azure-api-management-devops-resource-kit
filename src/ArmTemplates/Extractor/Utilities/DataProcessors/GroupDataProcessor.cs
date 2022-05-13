// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors.Absctraction;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Utilities.DataProcessors
{
    public class GroupDataProcessor: IGroupDataProcessor
    {
        public IDictionary<string, string> OverrideRules {
            get => this.overrideRules;  
            set => this.overrideRules = value;
        }

        IDictionary<string, string> overrideRules;

        public GroupDataProcessor() {
            this.OverrideRules = new Dictionary<string, string>() {
                { "Administrators", "administrators" },
                { "Developers", "developers" },
                { "Guests", "guests" }
            };
        }

        public void ProcessData(List<GroupTemplateResource> groupTemplates, ExtractorParameters extractorParameters) {
            
            foreach(var groupTemplate in groupTemplates) 
            {
                if (extractorParameters.OverrideGroupNames)
                {
                    this.OverrideName(groupTemplate);
                }
            }
            
        }

        public void OverrideName(GroupTemplateResource template)
        {
            if (this.OverrideRules == null)
            {
                return;
            }

            foreach (var rule in this.OverrideRules)
            {
                if (template.Properties.DisplayName.Equals(rule.Key))
                {
                    if (!template.Name.Equals(rule.Value))
                    {
                        template.OriginalName = template.Name;
                        template.NewName = rule.Value;
                        template.Name = rule.Value;
                        break;
                    }
                }
            }
        }
    }
}
