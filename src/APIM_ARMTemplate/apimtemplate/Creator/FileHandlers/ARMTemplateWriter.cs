using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ARMTemplateWriter
    {
        public void WriteJSONToFile(JObject json, string location)
        {
            using (StreamWriter file = File.CreateText(location))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                json.WriteTo(writer);
            }
        }

        public void WriteAPITemplateToFile(APITemplate template, string location)
        {
            WriteJSONToFile(JObject.FromObject(template), String.Concat(location, @"\APITemplate.json"));
        }
    }
}
