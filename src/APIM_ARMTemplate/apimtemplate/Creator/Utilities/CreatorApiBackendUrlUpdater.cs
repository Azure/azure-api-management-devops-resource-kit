using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Newtonsoft.Json;

namespace apimtemplate.Creator.Utilities
{
    public class CreatorApiBackendUrlUpdater
    {
        public CreatorConfig UpdateBackendServiceUrl(string backendServiceUrlFile, CreatorConfig creatorConfig)
        {
            // convert config file to CreatorConfig class
            FileReader fileReader = new FileReader();

            string backendurlConfigContent = fileReader.RetrieveLocalFileContents(backendServiceUrlFile);

            //if the file is json file
            if (fileReader.isJSON(backendurlConfigContent))
            {
                List<BackendUrlsConfig> backendUrls = JsonConvert.DeserializeObject<List<BackendUrlsConfig>>(backendurlConfigContent);

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
