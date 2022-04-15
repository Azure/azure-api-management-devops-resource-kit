// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Exceptions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Backend;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.Products;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities
{
    public class CreatorConfigurationValidator
    {
        readonly CreatorConfig creatorConfig;

        public CreatorConfigurationValidator(CreatorConfig creatorConfig)
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
            if (this.creatorConfig.namedValues != null)
            {
                foreach (var property in this.creatorConfig.namedValues)
                {
                    if (property.DisplayName == null)
                    {
                        isValid = false;
                        throw new CreatorConfigurationIsInvalidException("Display name is required is a Named Value is provided");
                    }
                }
            }
            return isValid;
        }

        bool ValidateProducts()
        {
            bool isValid = true;
            if (this.creatorConfig.products != null)
            {
                foreach (ProductsProperties product in this.creatorConfig.products)
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
            if (this.creatorConfig.loggers != null)
            {
                foreach (LoggerConfig logger in this.creatorConfig.loggers)
                {
                    if (logger.name == null)
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
            if (this.creatorConfig.backends != null)
            {
                foreach (BackendTemplateProperties backend in this.creatorConfig.backends)
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
            if (this.creatorConfig.authorizationServers != null)
            {
                foreach (var authorizationServer in this.creatorConfig.authorizationServers)
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
            if (this.creatorConfig.outputLocation == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("Output location is required");
            }
            if (this.creatorConfig.version == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("Version is required");
            }
            if (this.creatorConfig.apimServiceName == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("APIM service name is required");
            }
            if (this.creatorConfig.linked == true && this.creatorConfig.linkedTemplatesBaseUrl == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("LinkTemplatesBaseUrl is required for linked templates");
            }
            return isValid;
        }

        bool ValidateAPIs()
        {
            bool isValid = true;
            if (this.creatorConfig.apis == null)
            {
                isValid = false;
                throw new CreatorConfigurationIsInvalidException("API configuration is required");
            }
            foreach (APIConfig api in this.creatorConfig.apis)
            {
                if (api.name == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("API name is required");
                }
                if (api.openApiSpec == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("Open API Spec is required");
                }
                if (api.suffix == null)
                {
                    isValid = false;
                    throw new CreatorConfigurationIsInvalidException("API suffix is required");
                }
                if (api.operations != null)
                {
                    foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                    {
                        if (operation.Value == null || operation.Value.policy == null)
                        {
                            isValid = false;
                            throw new CreatorConfigurationIsInvalidException("Policy XML is required if an API operation is provided");
                        }
                    }
                }
                if (api.diagnostic != null && api.diagnostic.LoggerId == null)
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
            if (this.creatorConfig.apiVersionSets != null)
            {
                foreach (APIVersionSetConfig apiVersionSet in this.creatorConfig.apiVersionSets)
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
