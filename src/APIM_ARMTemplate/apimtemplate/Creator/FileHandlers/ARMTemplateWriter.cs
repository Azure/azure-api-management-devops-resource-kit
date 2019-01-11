using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ARMTemplateWriter
    {
        public void WriteJSONToFile(object template, string location)
        {
            string jsonString = JsonConvert.SerializeObject(template,
                            Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            File.WriteAllText(location, jsonString);
        }
    }
}
