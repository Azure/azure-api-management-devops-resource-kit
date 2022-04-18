// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiOperations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ApiSchemas;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Policy;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.ProductApis;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Tags;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Apis
{
    public class ApiTemplateResources : TemplateResourcesBase, ITemplateResources
    {
        public string FileName { get; set; }

        public List<ApiTemplateResource> Apis { get; set; } = new();

        public List<ApiSchemaTemplateResource> ApiSchemas { get; set; } = new();

        public List<DiagnosticTemplateResource> Diagnostics { get; set; } = new();

        public List<ProductApiTemplateResource> ApiProducts { get; set; } = new();

        public List<TagTemplateResource> Tags { get; set; } = new();

        public List<ApiOperationTemplateResource> ApiOperations { get; set; } = new();

        public List<PolicyTemplateResource> ApiOperationsPolicies { get; set; } = new();

        public List<TagTemplateResource> ApiOperationsTags { get; set; } = new();

        public List<PolicyTemplateResource> ApiPolicies { get; set; } = new();

        List<PolicyTemplateResource> allPoliciesStorage;

        public List<PolicyTemplateResource> GetAllPolicies()
        {
            if (!this.allPoliciesStorage.IsNullOrEmpty())
            {
                return this.allPoliciesStorage;
            }

            this.allPoliciesStorage = new List<PolicyTemplateResource>();

            if (!this.ApiPolicies.IsNullOrEmpty())
            {
                this.allPoliciesStorage.AddRange(this.ApiPolicies);
            }

            if (!this.ApiOperationsPolicies.IsNullOrEmpty())
            {
                this.allPoliciesStorage.AddRange(this.ApiOperationsPolicies);
            }

            return this.allPoliciesStorage;
        }

        public void AddResourcesData(ApiTemplateResources otherResources)
        {
            if (otherResources is null)
            {
                return;
            }

            if (!otherResources.Apis.IsNullOrEmpty())
            {
                this.Apis.AddRange(otherResources.Apis);
            }

            if (!otherResources.ApiSchemas.IsNullOrEmpty())
            {
                this.ApiSchemas.AddRange(otherResources.ApiSchemas);
            }

            if (!otherResources.Diagnostics.IsNullOrEmpty())
            {
                this.Diagnostics.AddRange(otherResources.Diagnostics);
            }

            if (!otherResources.ApiProducts.IsNullOrEmpty())
            {
                this.ApiProducts.AddRange(otherResources.ApiProducts);
            }

            if (!otherResources.Tags.IsNullOrEmpty())
            {
                this.Tags.AddRange(otherResources.Tags);
            }

            if (!otherResources.ApiOperations.IsNullOrEmpty())
            {
                this.ApiOperations.AddRange(otherResources.ApiOperations);
            }

            if (!otherResources.ApiOperationsPolicies.IsNullOrEmpty())
            {
                this.ApiOperationsPolicies.AddRange(otherResources.ApiOperationsPolicies);
            }

            if (!otherResources.ApiOperationsTags.IsNullOrEmpty())
            {
                this.ApiOperationsTags.AddRange(otherResources.ApiOperationsTags);
            }

            if (!otherResources.ApiPolicies.IsNullOrEmpty())
            {
                this.ApiPolicies.AddRange(otherResources.ApiPolicies);
            }
        }

        public TemplateResource[] BuildTemplateResources()
        {
            return this.ConcatenateTemplateResourcesCollections(
                this.Apis.ToArray(),
                this.ApiSchemas.ToArray(),
                this.Diagnostics.ToArray(),
                this.ApiProducts.ToArray(),
                this.Tags.ToArray(),
                this.ApiOperations.ToArray(),
                this.ApiOperationsPolicies.ToArray(),
                this.ApiOperationsTags.ToArray(),
                this.ApiPolicies.ToArray());
        }

        public bool HasContent()
        {
            return !this.Apis.IsNullOrEmpty()
                || !this.ApiSchemas.IsNullOrEmpty()
                || !this.Diagnostics.IsNullOrEmpty()
                || !this.ApiProducts.IsNullOrEmpty()
                || !this.Tags.IsNullOrEmpty()
                || !this.ApiOperations.IsNullOrEmpty()
                || !this.ApiOperationsPolicies.IsNullOrEmpty()
                || !this.ApiOperationsTags.IsNullOrEmpty()
                || this.ApiPolicies is not null;
        }
    }
}
