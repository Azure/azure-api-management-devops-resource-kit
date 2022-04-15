// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Abstractions
{
    public class Template<TTemplateResources> : Template
        where TTemplateResources : ITemplateResources, new()
    {
        /// <summary>
        /// Specifically typed resources of specific template type.
        /// Use it for explicit code type usage rather than base <cref="Resources"/> property of base class.
        /// It will not be serialized
        /// </summary>
        [JsonIgnore]
        public TTemplateResources TypedResources { get; set; }

        /// <summary>
        /// Returns TemplateResources collection by building specific resources of current template
        /// </summary>
        [JsonProperty(Order = 10)]
        public override TemplateResource[] Resources => this.TypedResources.BuildTemplateResources();

        /// <summary>
        /// Returns true, if template resources contains some data
        /// </summary>
        public new bool HasResources()
        {
            if (this.TypedResources is null)
            {
                return false;
            }

            return this.TypedResources.HasContent();
        }
    }
}
