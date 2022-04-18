// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Constants;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Groups;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.TemplateCreators
{
    public class ProductGroupTemplateCreator
    {
        public GroupTemplateResource CreateProductGroupTemplateResource(string groupName, string productName, string[] dependsOn)
        {
            // create products/apis resource with properties
            GroupTemplateResource productAPITemplateResource = new GroupTemplateResource()
            {
                Name = $"[concat(parameters('{ParameterNames.ApimServiceName}'), '/{productName}/{groupName}')]",
                Type = ResourceTypeConstants.ProductGroup,
                ApiVersion = GlobalConstants.ApiVersion,
                DependsOn = dependsOn,
                Properties = new GroupProperties()
            };
            return productAPITemplateResource;
        }

        public List<GroupTemplateResource> CreateProductGroupTemplateResources(ProductConfig product, string[] dependsOn)
        {
            // create a products/apis association resource for each product provided in the config file
            List<GroupTemplateResource> productGroupTemplates = new List<GroupTemplateResource>();
            // products is comma separated list of productIds
            string[] groupNames = product.groups.Split(", ");
            foreach (string groupName in groupNames)
            {
                GroupTemplateResource productAPITemplate = this.CreateProductGroupTemplateResource(groupName, product.Name, dependsOn);
                productGroupTemplates.Add(productAPITemplate);
            }
            return productGroupTemplates;
        }
    }
}
