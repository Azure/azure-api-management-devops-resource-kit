// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.FileHandlers;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities
{
    public class CreatorApiBackendUrlUpdater
    {
        public CreatorConfig UpdateBackendServiceUrl(string backendServiceUrlFile, CreatorConfig creatorConfig)
        {
            // convert config file to CreatorConfig class
            FileReader fileReader = new FileReader();

            string backendurlConfigContent = fileReader.RetrieveLocalFileContents(backendServiceUrlFile);

            //if the file is json file
            if (fileReader.IsJSON(backendurlConfigContent))
            {
                List<BackendUrlsConfig> backendUrls = backendurlConfigContent.Deserialize<List<BackendUrlsConfig>>();

                foreach (APIConfig aPIConfig in creatorConfig.apis)
                {
                    //if the apiname matches with the one in valid yaml file
                    BackendUrlsConfig backendUrlsConfig = backendUrls.Find(f => f.apiName == aPIConfig.name);

                    //update the backendurl as per the input json file
                    if (backendUrlsConfig != null && !string.IsNullOrEmpty(backendUrlsConfig.apiUrl))
                    {
                        aPIConfig.serviceUrl = backendUrlsConfig.apiUrl;
                    }
                }
            }

            return creatorConfig;
        }
    }
}
