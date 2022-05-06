// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models.Parameters;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities
{
    public class CreatorConfigurationValidator
    {
        readonly CreatorParameters creatorConfig;

        public CreatorConfigurationValidator(CreatorParameters creatorConfig)
        {
            this.creatorConfig = creatorConfig;
        }

        public bool ValidateCreatorConfig()
        {
            // ensure required parameters have been passed in
            if (this.ValidateBaseProperties() != true)
            {
                return false;
            }
            if (this.ValidateAPIs() != true)
            {
                return false;
            }
            if (this.ValidateAPIVersionSets() != true)
            {
                return false;
            }
            if (this.ValidateProducts() != true)
            {
                return false;
            }
            if (this.ValidateNamedValues() != true)
            {
                return false;
            }
            if (this.ValidateLoggers() != true)
            {
                return false;
            }
            if (this.ValidateAuthorizationServers() != true)
            {
                return false;
            }
            if (this.ValidateBackends() != true)
            {
                return false;
            }
            return true;
        }

        bool ValidateNamedValues()
        {
            bool isValid = true;
            if (this.creatorConfig.NamedValues != null)
            {
                foreach (var property in this.creatorConfig.NamedValues)
                {
                    if (property.DisplayName == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Display name is required if a Named Value is provided");
                    }
                }
            }
            return isValid;
        }

        bool ValidateProducts()
        {
            bool isValid = true;
            if (this.creatorConfig.Products != null)
            {
                foreach (ProductsProperties product in this.creatorConfig.Products)
                {
                    if (product.DisplayName == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Display name is required if an Product is provided");
                    }
                }
            }
            return isValid;
        }

        bool ValidateLoggers()
        {
            bool isValid = true;
            if (this.creatorConfig.Loggers != null)
            {
                foreach (LoggerConfig logger in this.creatorConfig.Loggers)
                {
                    if (logger.Name == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Name is required if an Logger is provided");
                    }
                }
            }
            return isValid;
        }

        bool ValidateBackends()
        {
            bool isValid = true;
            if (this.creatorConfig.Backends != null)
            {
                foreach (BackendTemplateProperties backend in this.creatorConfig.Backends)
                {
                    if (backend.Title == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Title is required if a Backend is provided");
                    }
                }
            }
            return isValid;
        }

        bool ValidateAuthorizationServers()
        {
            bool isValid = true;
            if (this.creatorConfig.AuthorizationServers != null)
            {
                foreach (var authorizationServer in this.creatorConfig.AuthorizationServers)
                {
                    if (authorizationServer.DisplayName == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Display name is required if an Authorization Server is provided");
                    }
                }
            }
            return isValid;
        }

        bool ValidateBaseProperties()
        {
            bool isValid = true;
            if (this.creatorConfig.OutputLocation == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("Output location is required");
            }
            if (this.creatorConfig.Version == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("Version is required");
            }
            if (this.creatorConfig.ApimServiceName == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("APIM service name is required");
            }
            if (this.creatorConfig.Linked == true && this.creatorConfig.LinkedTemplatesBaseUrl == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("LinkTemplatesBaseUrl is required for linked templates");
            }
            return isValid;
        }

        bool ValidateAPIs()
        {
            bool isValid = true;
            if (this.creatorConfig.Apis == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("API configuration is required");
            }
            foreach (ApiConfig api in this.creatorConfig.Apis)
            {
                if (api.Name == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("API name is required");
                }
                if (api.OpenApiSpec == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("Open API Spec is required");
                }
                if (api.Suffix == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("API suffix is required");
                }
                if (api.Operations != null)
                {
                    foreach (KeyValuePair<string, OperationsConfig> operation in api.Operations)
                    {
                        if (operation.Value == null || operation.Value.Policy == null)
                        {
                            isValid = false;
                            throw new CreatorConfigurationIsInvalidException("Policy XML is required if an API operation is provided");
                        }
                    }
                }
                if (api.Diagnostic != null && api.Diagnostic.LoggerId == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("LoggerId is required if an API diagnostic is provided");
                }
            }
            return isValid;
        }

        bool ValidateAPIVersionSets()
        {
            bool isValid = true;
            if (this.creatorConfig.ApiVersionSets != null)
            {
                foreach (ApiVersionSetConfig apiVersionSet in this.creatorConfig.ApiVersionSets)
                {
                    if (apiVersionSet != null && apiVersionSet.DisplayName == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Display name is required if an API Version Set is provided");
                    }
                }
            }
            return isValid;
        }
    }
}
