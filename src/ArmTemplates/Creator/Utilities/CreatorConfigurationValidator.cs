using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.TemplateModels;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities
{
    public class CreatorConfigurationValidator
    {
        CommandLineApplication commandLineApplication;

        public CreatorConfigurationValidator(CommandLineApplication commandLineApplication)
        {
            this.commandLineApplication = commandLineApplication;
        }

        public bool ValidateCreatorConfig(CreatorConfig creatorConfig)
        {
            // ensure required parameters have been passed in
            if (this.ValidateBaseProperties(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateAPIs(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateAPIVersionSets(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateProducts(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateNamedValues(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateLoggers(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateAuthorizationServers(creatorConfig) != true)
            {
                return false;
            }
            if (this.ValidateBackends(creatorConfig) != true)
            {
                return false;
            }
            return true;
        }

        public bool ValidateNamedValues(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.namedValues != null)
            {
                foreach (PropertyResourceProperties property in creatorConfig.namedValues)
                {
                    if (property.displayName == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this.commandLineApplication, "Display name is required is a Named Value is provided");
                    }
                }
            }
            return isValid;
        }
        public bool ValidateProducts(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.products != null)
            {
                foreach (ProductsTemplateProperties product in creatorConfig.products)
                {
                    if (product.displayName == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this.commandLineApplication, "Display name is required if an Product is provided");
                    }
                }
            }
            return isValid;
        }

        public bool ValidateLoggers(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.loggers != null)
            {
                foreach (LoggerConfig logger in creatorConfig.loggers)
                {
                    if (logger.name == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this.commandLineApplication, "Name is required if an Logger is provided");
                    }
                }
            }
            return isValid;
        }

        public bool ValidateBackends(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.backends != null)
            {
                foreach (BackendTemplateProperties backend in creatorConfig.backends)
                {
                    if (backend.title == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this.commandLineApplication, "Title is required if a Backend is provided");
                    }
                }
            }
            return isValid;
        }

        public bool ValidateAuthorizationServers(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.authorizationServers != null)
            {
                foreach (AuthorizationServerTemplateProperties authorizationServer in creatorConfig.authorizationServers)
                {
                    if (authorizationServer.displayName == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this.commandLineApplication, "Display name is required if an Authorization Server is provided");
                    }
                }
            }
            return isValid;
        }

        public bool ValidateBaseProperties(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.outputLocation == null)
            {
                isValid = false;
                throw new CommandParsingException(this.commandLineApplication, "Output location is required");
            }
            if (creatorConfig.version == null)
            {
                isValid = false;
                throw new CommandParsingException(this.commandLineApplication, "Version is required");
            }
            if (creatorConfig.apimServiceName == null)
            {
                isValid = false;
                throw new CommandParsingException(this.commandLineApplication, "APIM service name is required");
            }
            if (creatorConfig.linked == true && creatorConfig.linkedTemplatesBaseUrl == null)
            {
                isValid = false;
                throw new CommandParsingException(this.commandLineApplication, "LinkTemplatesBaseUrl is required for linked templates");
            }
            return isValid;
        }

        public bool ValidateAPIs(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.apis == null)
            {
                isValid = false;
                throw new CommandParsingException(this.commandLineApplication, "API configuration is required");
            }
            foreach (APIConfig api in creatorConfig.apis)
            {
                if (api.name == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this.commandLineApplication, "API name is required");
                }
                if (api.openApiSpec == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this.commandLineApplication, "Open API Spec is required");
                }
                if (api.suffix == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this.commandLineApplication, "API suffix is required");
                }
                if (api.operations != null)
                {
                    foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                    {
                        if (operation.Value == null || operation.Value.policy == null)
                        {
                            isValid = false;
                            throw new CommandParsingException(this.commandLineApplication, "Policy XML is required if an API operation is provided");
                        }
                    }
                }
                if (api.diagnostic != null && api.diagnostic.loggerId == null)
                {
                    isValid = false;
                    throw new CommandParsingException(this.commandLineApplication, "LoggerId is required if an API diagnostic is provided");
                }
            }
            return isValid;
        }

        public bool ValidateAPIVersionSets(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.apiVersionSets != null)
            {
                foreach (APIVersionSetConfig apiVersionSet in creatorConfig.apiVersionSets)
                {
                    if (apiVersionSet != null && apiVersionSet.displayName == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(this.commandLineApplication, "Display name is required if an API Version Set is provided");
                    }
                }
            }
            return isValid;
        }
    }
}
