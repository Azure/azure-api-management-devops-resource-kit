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
            JObject json = JObject.FromObject(template);
            using (StreamWriter file = File.CreateText(location))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                json.WriteTo(writer);
            }
        }
    }
}
