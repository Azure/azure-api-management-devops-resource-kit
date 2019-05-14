
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using System.Collections.Generic;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class CreatorConfigurationValidator
    {
        private CommandLineApplication commandLineApplication;

        public CreatorConfigurationValidator(CommandLineApplication commandLineApplication)
        {
            this.commandLineApplication = commandLineApplication;
        }

        public bool ValidateCreatorConfig(CreatorConfig creatorConfig)
        {
            // ensure required parameters have been passed in
            if (ValidateBaseProperties(creatorConfig) != true)
            {
                return false;
            }
            if (ValidateAPIs(creatorConfig) != true)
            {
                return false;
            }
            if (ValidateAPIVersionSets(creatorConfig) != true)
            {
                return false;
            }
            if (ValidateProducts(creatorConfig) != true)
            {
                return false;
            }
            if (ValidateLoggers(creatorConfig) != true)
            {
                return false;
            }
            if (ValidateAuthorizationServers(creatorConfig) != true)
            {
                return false;
            }
            if (ValidateBackends(creatorConfig) != true)
            {
                return false;
            }
            return true;
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
                        throw new CommandParsingException(commandLineApplication, "Display name is required if an Product is provided");
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
                        throw new CommandParsingException(commandLineApplication, "Name is required if an Logger is provided");
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
                        throw new CommandParsingException(commandLineApplication, "Title is required if a Backend is provided");
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
                        throw new CommandParsingException(commandLineApplication, "Display name is required if an Authorization Server is provided");
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
                throw new CommandParsingException(commandLineApplication, "Output location is required");
            }
            if (creatorConfig.version == null)
            {
                isValid = false;
                throw new CommandParsingException(commandLineApplication, "Version is required");
            }
            if (creatorConfig.apimServiceName == null)
            {
                isValid = false;
                throw new CommandParsingException(commandLineApplication, "APIM service name is required");
            }
            if (creatorConfig.linked == true && creatorConfig.linkedTemplatesBaseUrl == null)
            {
                isValid = false;
                throw new CommandParsingException(commandLineApplication, "LinkTemplatesBaseUrl is required for linked templates");
            }
            return isValid;
        }

        public bool ValidateAPIs(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if (creatorConfig.apis == null)
            {
                isValid = false;
                throw new CommandParsingException(commandLineApplication, "API configuration is required");
            }
            foreach (APIConfig api in creatorConfig.apis)
            {
                if (api.name == null)
                {
                    isValid = false;
                    throw new CommandParsingException(commandLineApplication, "API name is required");
                }
                if (api.openApiSpec == null)
                {
                    isValid = false;
                    throw new CommandParsingException(commandLineApplication, "Open API Spec is required");
                }
                if (api.suffix == null)
                {
                    isValid = false;
                    throw new CommandParsingException(commandLineApplication, "API suffix is required");
                }
                if (api.operations != null)
                {
                    foreach (KeyValuePair<string, OperationsConfig> operation in api.operations)
                    {
                        if (operation.Value == null || operation.Value.policy == null)
                        {
                            isValid = false;
                            throw new CommandParsingException(commandLineApplication, "Policy XML is required if an API operation is provided");
                        }
                    }
                }
                if (api.diagnostic != null && api.diagnostic.loggerId == null)
                {
                    isValid = false;
                    throw new CommandParsingException(commandLineApplication, "LoggerId is required if an API diagnostic is provided");
                }
            }
            return isValid;
        }

        public bool ValidateAPIVersionSets(CreatorConfig creatorConfig)
        {
            bool isValid = true;
            if(creatorConfig.apiVersionSets != null)
            {
                foreach (APIVersionSetConfig apiVersionSet in creatorConfig.apiVersionSets)
                {
                    if (apiVersionSet != null && apiVersionSet.displayName == null)
                    {
                        isValid = false;
                        throw new CommandParsingException(commandLineApplication, "Display name is required if an API Version Set is provided");
                    }
                }
            }
            return isValid;
        }
    }
}
