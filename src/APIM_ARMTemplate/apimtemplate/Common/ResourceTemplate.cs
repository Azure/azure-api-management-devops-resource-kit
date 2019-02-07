using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIManagementTemplate.Models
{
    public class ResourceTemplate
    {
        private List<string> names = new List<string>();
        public void AddName(string name)
        {
            names.Add(name);
        }
        public string GetResourceId()
        {
            return String.Join(",", names);
        }

        public string comments { get; set; }
        public string type { get; set; }
        public string name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    return "[concat(" + String.Join(",'/',", names) + ")]";
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _name = null;

        public string apiVersion
        {
            get
            {
                return "2017-03-01";
            }
        }
        public JObject properties { get; set; }

        public IList<JObject> resources { get; set; }

        public JArray dependsOn { get; set; }

        public ResourceTemplate()
        {
            properties = new JObject();
            resources = new List<JObject>();
            dependsOn = new JArray();
        }
    }
}
