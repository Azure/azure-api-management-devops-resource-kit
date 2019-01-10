using Microsoft.Azure.Management.ApiManagement.ArmTemplates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace APIManagementTemplate.Models
{
    public class DeploymentTemplate
    {

        [JsonProperty("$schema")]
        public string schema
        {
            get
            {
                return Constants.deploymentSchema;
            }
        }
        public string contentVersion
        {
            get
            {
                return "1.0.0.0";
            }
        }

        public JObject parameters { get; set; }
        public JObject variables { get; set; }

        public IList<JObject> resources { get; set; }

        public JObject outputs { get; set; }

        private bool parametrizePropertiesOnly { get; set; }
        private bool fixedServiceNameParameter { get; set; }
        private bool referenceApplicationInsightsInstrumentationKey { get; set; }

        public DeploymentTemplate(bool parametrizePropertiesOnly = false, bool fixedServiceNameParameter = false, bool referenceApplicationInsightsInstrumentationKey = false)
        {
            parameters = new JObject();
            variables = new JObject();
            resources = new List<JObject>();
            outputs = new JObject();

            this.parametrizePropertiesOnly = parametrizePropertiesOnly;
            this.fixedServiceNameParameter = fixedServiceNameParameter;
            this.referenceApplicationInsightsInstrumentationKey = referenceApplicationInsightsInstrumentationKey;
        }

        public static DeploymentTemplate FromString(string template)
        {
            return JsonConvert.DeserializeObject<DeploymentTemplate>(template);
        }


        public void RemoveResources_OfType(string type)
        {
            var resources = this.resources.Where(rr => rr.Value<string>("type") == type);
            int count = resources.Count();
            for (int i = 0; i < count; i++)
            {
                RemoveResource(resources.First());
            }
        }


        private void RemoveResource(JObject resource)
        {
            this.parameters.Remove(resource.Value<string>("name").Replace("[parameters('", "").Replace("')]", ""));
            this.resources.Remove(resource);
        }

        public string AddParameter(string paramname, string type, object defaultvalue)
        {
            return AddParameter(paramname, type, new JProperty("defaultValue", defaultvalue));
        }


        public string AddParameter(string paramname, string type, JProperty defaultvalue)
        {
            string realParameterName = paramname;
            JObject param = new JObject();
            param.Add("type", JToken.FromObject(type));
            param.Add(defaultvalue);

            if (this.parameters[paramname] == null)
            {
                this.parameters.Add(paramname, param);
            }
            else
            {
                if (!this.parameters[paramname].Value<string>("defaultValue").Equals(defaultvalue.Value.ToString()))
                {
                    foreach (var p in this.parameters)
                    {
                        if (p.Key.StartsWith(paramname))
                        {
                            for (int i = 2; i < 100; i++)
                            {
                                realParameterName = paramname + i.ToString();
                                if (this.parameters[realParameterName] == null)
                                {
                                    this.parameters.Add(realParameterName, param);
                                    return realParameterName;
                                }
                            }
                        }
                    }
                }
            }
            return realParameterName;
        }

        public string AddVariable(string variablename, string value)
        {
            string realVariableName = variablename;

            if (this.variables[variablename] == null)
            {
                this.variables.Add(variablename, value);
            }
            else
            {
                foreach (var p in this.variables)
                {
                    if (p.Key.StartsWith(variablename))
                    {
                        for (int i = 2; i < 100; i++)
                        {
                            realVariableName = variablename + i.ToString();
                            if (this.variables[realVariableName] == null)
                            {
                                this.variables.Add(realVariableName, value);
                                return realVariableName;
                            }
                        }
                    }
                }
            }
            return realVariableName;
        }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string WrapParameterName(string paramname)
        {
            return "[parameters('" + paramname + "')]";
        }
        public string RemoveWrapParameter(string parameterstring)
        {
            return parameterstring.Replace("[parameters('", "").Replace("')]", "");
        }

        private string GetParameterName(string name, string ending)
        {
            if (!name.StartsWith("[parameters('"))
                return name;

            var tmpname = RemoveWrapParameter(name);
            if (!string.IsNullOrEmpty(ending) && tmpname.Contains(ending))
            {
                tmpname = tmpname.Substring(0, tmpname.IndexOf("_name"));
            }
            return tmpname;
        }

        public void AddParameterFromObject(JObject obj, string propertyName, string propertyType, string paramNamePrefix = "")
        {

            var propValue = (string)obj[propertyName];
            if (propValue == null || (propValue.StartsWith("[") && propValue.EndsWith("]")))
                return;
            obj[propertyName] = WrapParameterName(this.AddParameter(paramNamePrefix + "_" + propertyName, propertyType, obj[propertyName]));
        }

        /**
         * 
         *  API Management pecifics
         * 
         */

        private bool APIMInstanceAdded = false;
        private string apimservicename;

        public JObject AddAPIManagementInstance(JObject restObject)
        {
            if (restObject == null)
                return null;

            string servicename = restObject.Value<string>("name").ToLowerInvariant();
            string type = restObject.Value<string>("type");
            apimservicename = servicename;
            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = WrapParameterName(AddParameter($"{GetServiceName(servicename)}", "string", servicename));
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["sku"] = restObject["sku"];
            resource["sku"]["name"] = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_sku_name", "string", restObject["sku"].Value<string>("name")));
            resource["sku"]["capacity"] = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_sku_capacity", "string", restObject["sku"].Value<string>("capacity")));
            resource["location"] = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_location", "string", restObject.Value<string>("location")));
            resource["tags"] = restObject["tags"];
            resource["scale"] = null;
            resource["properties"] = new JObject();
            resource["properties"]["publisherEmail"] = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_publisherEmail", "string", restObject["properties"].Value<string>("publisherEmail")));
            resource["properties"]["publisherName"] = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_publisherName", "string", restObject["properties"].Value<string>("publisherName")));
            resource["properties"]["notificationSenderEmail"] = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_notificationSenderEmail", "string", restObject["properties"].Value<string>("notificationSenderEmail")));
            resource["properties"]["hostnameConfigurations"] = restObject["properties"]["hostnameConfigurations"];
            resource["properties"]["additionalLocations"] = restObject["properties"]["additionalLocations"];
            resource["properties"]["virtualNetworkConfiguration"] = restObject["properties"]["virtualNetworkConfiguration"];
            resource["properties"]["customProperties"] = restObject["properties"]["customProperties"];
            resource["properties"]["virtualNetworkType"] = restObject["properties"]["virtualNetworkType"];
            this.resources.Add(resource);
            APIMInstanceAdded = true;
            return resource;
        }

        public string GetServiceName(string servicename, bool addName = true)
        {
            if (fixedServiceNameParameter)
                return "apimServiceName";
            return addName ? $"service_{servicename}_name" : $"service_{servicename}";
        }

        public JObject AddApi(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");
            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = parametrizePropertiesOnly ? $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/{name}')]" : $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/' ,parameters('{AddParameter($"api_{name}_name", "string", name)}'))]";
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            AddParameterFromObject((JObject)resource["properties"], "apiRevision", "string", name);
            AddParameterFromObject((JObject)resource["properties"], "serviceUrl", "string", name);
            AddParameterFromObject((JObject)resource["properties"], "apiVersion", "string", name);
            AddParameterFromObject((JObject)resource["properties"], "isCurrent", "bool", name);

            if (APIMInstanceAdded)
            {
                resource["dependsOn"] = new JArray(new string[] { $"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]" });
            }
            else
            {
                resource["dependsOn"] = new JArray(); ;
            }

            this.resources.Add(resource);
            return resource;
        }

        public ResourceTemplate CreateAPISchema(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");
            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");
            string apiname = apiid.ValueAfter("apis");
            apiname = parametrizePropertiesOnly ? $"'{apiname}'" : $"parameters('{AddParameter($"api_{apiname}_name", "string", apiname)}')";

            var obj = new ResourceTemplate();
            obj.AddName($"parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}')");
            obj.AddName(apiname);
            obj.AddName($"'{name}'");

            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.type = type;
            obj.properties = restObject.Value<JObject>("properties");

            if (APIMInstanceAdded)
            {
                obj.dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");
                //resource["dependsOn"] = new JArray(new string[] { $"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]" });
            }
            obj.dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{GetServiceName(servicename)}'),{apiname})]");

            return obj;
        }

        public JObject CreateOperation(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");

            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");
            string apiname = apiid.ValueAfter("apis");

            name = $"'{name}'";
            apiname = parametrizePropertiesOnly ? $"'{apiname}'" : $"parameters('{AddParameter($"api_{apiname}_name", "string", apiname)}')";

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/', {apiname}, '/', {name})]";
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            //Schema list
            var schemalist = new List<string>();

            var request = resource["properties"].Value<JObject>("request");
            if (request != null)
            {
                schemalist = schemalist.Union(FixRepresentations(request.Value<JArray>("representations"))).ToList();
            }

            var responses = resource["properties"].Value<JArray>("responses");
            if (responses != null)
            {
                foreach (var resp in responses)
                {
                    schemalist = schemalist.Union(FixRepresentations(resp.Value<JArray>("representations"))).ToList();
                }
            }


            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");
            }
            foreach (var schema in schemalist)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis/schemas', parameters('{GetServiceName(servicename)}'),{apiname},'{schema}')]");
            }

            dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{GetServiceName(servicename)}'),{apiname})]");

            resource["dependsOn"] = dependsOn;

            return resource;
            //this.resources.Add(resource);
        }

        private List<string> FixRepresentations(JArray reps)
        {
            var ll = new List<string>();
            if (reps == null)
                return ll;
            foreach (JObject rep in reps)
            {
                string sample = rep.Value<string>("sample") ?? "";
                //if sample is an arrau and start with [ it need's to be escaped
                if (sample.StartsWith("["))
                    rep["sample"] = "[" + sample;

                string generatedSample = rep.Value<string>("generatedSample") ?? "";
                if (generatedSample.StartsWith("["))
                    rep["generatedSample"] = "[" + generatedSample;


                var schema = rep.Value<string>("schemaId");
                if (!string.IsNullOrEmpty(schema))
                    ll.Add(schema);
            }
            return ll;
        }

        public Property AddBackend(JObject restObject, JObject azureResource)
        {
            Property retval = null;
            if (restObject == null)
                return retval;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");

            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/' ,'{name}')]";
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            var dependsOn = new JArray();
            if (restObject["properties"]["resourceId"] != null)
            {
                string resourceid = restObject["properties"].Value<string>("resourceId");
                var aid = new AzureResourceId(resourceid.Replace("https://management.azure.com/", ""));
                aid.SubscriptionId = "',subscription().subscriptionId,'";
                var rgparamname = AddParameter(name + "_resourceGroup", "string", aid.ResourceGroupName);
                aid.ResourceGroupName = "',parameters('" + rgparamname + "'),'";
                if (resourceid.Contains("providers/Microsoft.Logic/workflows")) //Logic App
                {
                    var laname = aid.ValueAfter("workflows");
                    var logicappname = AddParameter(name + "_logicAppName", "string", laname);
                    aid.ReplaceValueAfter("workflows", "',parameters('" + logicappname + "')");


                    var triggerObject = azureResource["properties"]["definition"].Value<JObject>("triggers");
                    string triggername = "manual";
                    foreach (var trigger in triggerObject)
                    {
                        if (trigger.Value.Value<string>("type") == "Request" && trigger.Value.Value<string>("kind") == "Http")
                        {
                            triggername = trigger.Key;
                        }
                    }
                    //need to get the Logic App triggers and find the HTTP one....

                    string listcallbackref = $"listCallbackUrl(resourceId(parameters('{rgparamname}'), 'Microsoft.Logic/workflows/triggers', parameters('{logicappname}'), '{triggername}'), '2017-07-01')";

                    resource["properties"]["url"] = $"[substring({listcallbackref}.basePath,0,add(10,indexOf({listcallbackref}.basePath,'/triggers/')))]";
                    retval = new Property()
                    {
                        type = Property.PropertyType.LogicApp,
                        name = laname.ToLower(),
                        extraInfo = listcallbackref
                    };
                }
                else if (resourceid.Contains("providers/Microsoft.Web/sites")) //Web App/Function
                {
                    var sitename = aid.ValueAfter("sites");
                    var paramsitename = AddParameter(name + "_siteName", "string", sitename);
                    aid.ReplaceValueAfter("sites", "',parameters('" + paramsitename + "')");
                    resource["properties"]["description"] = $"[parameters('{paramsitename}')]";
                    resource["properties"]["url"] = $"[concat('https://',toLower(parameters('{paramsitename}')),'.azurewebsites.net/')]";
                    retval = new Property()
                    {
                        type = Property.PropertyType.Function,
                        name = sitename.ToLower(),
                        extraInfo = $"listsecrets(resourceId(parameters('{rgparamname}'),'Microsoft.Web/sites/functions', parameters('{paramsitename}'), 'replacewithfunctionoperationname'),'2015-08-01').key"
                    };
                    var code = (resource["properties"]?["credentials"]?["query"]?.Value<JArray>("code") ?? new JArray()).FirstOrDefault();
                    if (code != null)
                    {
                        var value = code.Value<string>();
                        if (value.StartsWith("{{") && value.EndsWith("}}"))
                        {
                            var parsed = value.Substring(2, value.Length - 4);
                            dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/properties', parameters('{GetServiceName(servicename)}'),'{parsed}')]");
                        }
                    }
                }
                resource["properties"]["resourceId"] = "[concat('https://management.azure.com/','" + aid.ToString().Substring(1) + ")]";
            }
            else
            {
                AddParameterFromObject((JObject)resource["properties"], "url", "string", name);
            }

            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");
                resource["dependsOn"] = dependsOn;
            }

            if (this.resources.Where(rr => rr.Value<string>("name") == obj.name).Count() == 0)
                this.resources.Add(resource);

            return retval;
        }
        public ResourceTemplate AddVersionSet(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");


            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.AddName($"parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}')");
            obj.AddName($"'{name}'");

            obj.type = type;
            obj.properties = restObject.Value<JObject>("properties");
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");

            }
            resource["dependsOn"] = dependsOn;
            this.resources.Add(resource);
            return obj;
        }

        public void AddGroup(JObject restObject)
        {
            if (restObject == null)
                return;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");


            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = parametrizePropertiesOnly ? $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/{name}')]" : $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/' ,parameters('{AddParameter($"group_{name}_name", "string", name)}'))]";
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");

            }
            resource["dependsOn"] = dependsOn;

            // Avoid duplicates.
            if (this.resources.Count(rr => rr.Value<string>("name") == obj.name && rr.Value<string>("type") == obj.type) == 0)
            {
                this.resources.Add(resource);
            }
        }

        public JObject AddProduct(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");

            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = parametrizePropertiesOnly ? $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/{name}')]" : $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/' ,parameters('{AddParameter($"product_{name}_name", "string", name)}'))]";
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");

            }
            resource["dependsOn"] = dependsOn;

            this.resources.Add(resource);
            return resource;
        }

        public JObject AddProductSubObject(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");

            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");
            string productname = apiid.ValueAfter("products");

            productname = parametrizePropertiesOnly ? $"'{productname}'" : $"parameters('{AddParameter($"product_{productname}_name", "string", productname)}')";
            bool nonApi = type != "Microsoft.ApiManagement/service/products/apis";
            var objectname = parametrizePropertiesOnly || nonApi ? $"'{name}'" : $"parameters('{AddParameter($"api_{name}_name", "string", name)}')";

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.name = $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/', {productname}, '/', {objectname})]";
            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            var dependsOn = new JArray();
            dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/products', parameters('{GetServiceName(servicename)}'), {productname})]");
            resource["dependsOn"] = dependsOn;

            return resource;
        }

        public ResourceTemplate AddProperty(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");
            bool secret = restObject["properties"].Value<bool>("secret");

            AzureResourceId apiid = new AzureResourceId(restObject.Value<string>("id"));
            string servicename = apiid.ValueAfter("service");

            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            obj.AddName($"parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}')");
            obj.AddName($"'{name}'");

            obj.type = type;
            obj.properties = restObject.Value<JObject>("properties");
            var resource = JObject.FromObject(obj);

            var propValue = resource["properties"].Value<string>("value");
            if (!(propValue == null || (propValue.StartsWith("[") && propValue.EndsWith("]"))))
            {
                resource["properties"]["value"] = WrapParameterName(this.AddParameter(restObject["properties"].Value<string>("displayName") + "_" + "value", secret ? "securestring" : "string", secret ? "secretvalue" : resource["properties"]["value"]));
            }



            //AddParameterFromObject((JObject)resource["properties"], "value", secret ? "securestring" : "string", restObject["properties"].Value<string>("displayName"));

            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");

            }
            resource["dependsOn"] = dependsOn;

            this.resources.Add(resource);
            return obj;
        }

        //need to return an object with property list and so on
        public JObject CreatePolicy(JObject restObject)
        {
            if (restObject == null)
                return null;

            string name = restObject.Value<string>("name");
            string type = restObject.Value<string>("type");
            string servicename = "";
            string apiname = "";
            string operationname = "";
            bool servicePolicy = false;

            name = $"'{name}'";


            var rid = new AzureResourceId(restObject.Value<string>("id"));
            var obj = new ResourceTemplate();
            obj.comments = "Generated for resource " + restObject.Value<string>("id");
            if (type == "Microsoft.ApiManagement/service/apis/policies")
            {
                servicename = rid.ValueAfter("service");
                apiname = rid.ValueAfter("apis");

                apiname = parametrizePropertiesOnly ? $"'{apiname}'" : $"parameters('{AddParameter($"api_{apiname}_name", "string", apiname)}')";
                obj.name = $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/', {apiname}, '/', {name})]";
            }
            else if (type == "Microsoft.ApiManagement/service/apis/operations/policies")
            {
                servicename = rid.ValueAfter("service");
                apiname = rid.ValueAfter("apis");
                operationname = rid.ValueAfter("operations");
                apiname = parametrizePropertiesOnly ? $"'{apiname}'" : $"parameters('{AddParameter($"api_{apiname}_name", "string", apiname)}')";
                operationname = $"'{operationname}'";
                obj.name = $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/', {apiname}, '/', {operationname}, '/', {name})]";
            }
            else if (type == "Microsoft.ApiManagement/service/policies")
            {
                servicename = rid.ValueAfter("service");
                obj.name = $"[concat(parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}'), '/', 'policy')]";
                servicePolicy = true;
            }

            obj.type = type;
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];

            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");
            }

            if (!servicePolicy)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis', parameters('{GetServiceName(servicename)}') , {apiname})]");
            }

            if (type == "Microsoft.ApiManagement/service/apis/operations/policies")
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service/apis/operations', parameters('{GetServiceName(servicename)}'), {apiname}, {operationname})]");
            }

            resource["dependsOn"] = dependsOn;
            return resource;
            //this.resources.Add(resource);
        }

        public JObject AddApplicationInsightsInstance(JObject restObject)
        {
            var obj = new ResourceTemplate
            {
                comments = "Generated for resource " + restObject.Value<string>("id"),
                type = "Microsoft.Insights/components",
            };
            var rid = new AzureResourceId(restObject.Value<string>("id"));
            var servicename = rid.ValueAfter("service");
            string name = restObject.Value<string>("name");
            obj.name = WrapParameterName(AddParameter($"{GetServiceName(servicename, false)}_applicationInsights", "string", name));
            var resource = JObject.FromObject(obj);
            resource["location"] = WrapParameterName($"{GetServiceName(servicename, false)}_location");
            resource["apiVersion"] = "2015-05-01";
            resource["kind"] = "other";
            resource["properties"] = JObject.FromObject(new { Application_Type = "other" });
            if (APIMInstanceAdded)
            {
                resource["dependsOn"] = new JArray
                {
                    $"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]"
                };
            }
            return resource;
        }

        public JObject CreateServiceResource(JObject restObject, string resourceType, bool addResource)
        {
            var obj = new ResourceTemplate
            {
                comments = "Generated for resource " + restObject.Value<string>("id"),
                type = resourceType
            };
            var rid = new AzureResourceId(restObject.Value<string>("id"));
            var servicename = rid.ValueAfter("service");
            obj.AddName($"parameters('{AddParameter($"{GetServiceName(servicename)}", "string", servicename)}')");
            var name = restObject.Value<string>("name");
            var resourceTypeShort = GetResourceTypeShort(resourceType);
            bool applicationInsightsLogger = IsApplicationInsightsLogger(restObject);
            if (applicationInsightsLogger && referenceApplicationInsightsInstrumentationKey)
                name = $"parameters('{AddParameter($"{GetServiceName(servicename, false)}_applicationInsights", "string", name)}')";
            else
                name = parametrizePropertiesOnly ? $"'{name}'" : $"parameters('{AddParameter($"{resourceTypeShort}_{name}_name", "string", name)}')";
            obj.AddName(name);
            var resource = JObject.FromObject(obj);
            resource["properties"] = restObject["properties"];
            var dependsOn = new JArray();
            if (APIMInstanceAdded)
            {
                dependsOn.Add($"[resourceId('Microsoft.ApiManagement/service', parameters('{GetServiceName(servicename)}'))]");
            }
            resource["dependsOn"] = dependsOn;
            if (addResource)
            {
                if (resources.All(x => x.Value<string>("name") != resource.Value<string>("name")))
                    resources.Add(resource);
            }

            return resource;
        }

        private bool IsApplicationInsightsLogger(JObject restObject)
        {
            if (restObject.Value<string>("type") != "Microsoft.ApiManagement/service/loggers")
                return false;
            if (restObject["properties"]?["loggerType"]?.Value<string>() != "applicationInsights")
                return false;
            return true;
        }

        private string GetResourceTypeShort(string resourceType)
        {
            var split = resourceType.Split('/');
            var type = split[split.Length - 1];
            if (type.EndsWith("s"))
                return type.Substring(0, type.Length - 1);
            return type;
        }

        public JObject CreateCertificate(JObject restObject, bool addResource)
        {
            var resource = CreateServiceResource(restObject, "Microsoft.ApiManagement/service/certificates", addResource);
            var certificatename = new AzureResourceId(restObject.Value<string>("id")).ValueAfter("certificates");
            var properties = new
            {
                data = WrapParameterName(AddParameter($"certificate_{certificatename}_data", "securestring", String.Empty)),
                password = WrapParameterName(AddParameter($"certificate_{certificatename}_password", "securestring", String.Empty))
            };
            resource["properties"] = JObject.FromObject(properties);
            return resource;
        }

        public JObject CreateLogger(JObject restObject, bool addResource)
        {
            var resource = CreateServiceResource(restObject, "Microsoft.ApiManagement/service/loggers", addResource);
            var azureResourceId = new AzureResourceId(restObject.Value<string>("id"));
            var loggerName = azureResourceId.ValueAfter("loggers");
            var serviceName = azureResourceId.ValueAfter("service");
            var credentials = resource["properties"]?["credentials"];
            if (credentials != null)
            {
                if (credentials.Value<string>("connectionString") != null)
                {
                    credentials["connectionString"] = WrapParameterName(AddParameter($"logger_{loggerName}_connectionString", "securestring", String.Empty));
                }
                var loggerType = resource["properties"]?["loggerType"]?.Value<string>() ?? string.Empty;
                if (referenceApplicationInsightsInstrumentationKey && loggerType == "applicationInsights" && credentials.Value<string>("instrumentationKey") != null)
                {
                    string parameter = AddParameter($"{GetServiceName(serviceName, false)}_applicationInsights", "string", loggerName);
                    credentials["instrumentationKey"] = $"[reference(resourceId('Microsoft.Insights/components', parameters('{parameter}')), '2014-04-01').InstrumentationKey]";
                    var dependsOn = resource.Value<JArray>("dependsOn") ?? new JArray();
                    dependsOn.Add($"[resourceId('Microsoft.Insights/components',parameters('{GetServiceName(serviceName, false)}_applicationInsights'))]");
                    resource["dependsOn"] = dependsOn;
                    if (resource["properties"] is JObject properties)
                    {
                        properties.Remove("resourceId");
                    }
                }
                if (credentials.Value<string>("name") != null)
                {
                    credentials["name"] = WrapParameterName(AddParameter($"logger_{loggerName}_credentialName", "string", GetDefaultValue(resource, "credentials", "name")));
                }
            }
            return resource;
        }

        public JObject CreateOpenIDConnectProvider(JObject restObject, bool addResource)
        {
            var resource = CreateServiceResource(restObject, "Microsoft.ApiManagement/service/openidConnectProviders", addResource);
            var providerName = new AzureResourceId(restObject.Value<string>("id")).ValueAfter("openidConnectProviders");
            resource["properties"]["displayName"] = WrapParameterName(AddParameter($"openidConnectProvider_{providerName}_displayname", "string", GetDefaultValue(resource, "displayName")));
            resource["properties"]["metadataEndpoint"] = WrapParameterName(AddParameter($"openidConnectProvider_{providerName}_metadataEndpoint", "string", GetDefaultValue(resource, "metadataEndpoint")));
            resource["properties"]["clientId"] = WrapParameterName(AddParameter($"openidConnectProvider_{providerName}_clientId", "string", GetDefaultValue(resource, "clientId")));
            resource["properties"]["clientSecret"] = WrapParameterName(AddParameter($"openidConnectProvider_{providerName}_clientSecret", "securestring", String.Empty));
            return resource;
        }

        private static string GetDefaultValue(JObject resource, params string[] names)
        {
            var prop = resource["properties"];
            foreach (var name in names)
            {
                prop = prop[name];
                if (prop == null)
                    return string.Empty;
            }
            return prop.Value<string>() ?? String.Empty;
        }

        public JObject CreateBackend(JObject restObject)
        {
            var resource = CreateServiceResource(restObject, "Microsoft.ApiManagement/service/backends", false);
            return resource;
        }

        public void RemoveResources_BuiltInGroups()
        {
            var groups = this.resources.Where(rr => rr.Value<string>("type") == "Microsoft.ApiManagement/service/groups" && rr["properties"].Value<string>("type") == "system");
            int count = groups.Count();
            for (int i = 0; i < count; i++)
            {
                RemoveResource(groups.First());

            }
        }

        public void ParameterizeAPIs()
        {
            var apis = this.resources.Where(rr => rr.Value<string>("type") == "Microsoft.ApiManagement/service/apis");

            foreach (var api in apis)
            {

                var name = GetParameterName(((string)api["name"]), "_name");
                AddParameterFromObject((JObject)api["properties"], "serviceUrl", "string", name);
                AddParameterFromObject((JObject)api["properties"], "apiRevision", "string", name);
            }
        }

        public void ParameterizeBackends()
        {
            var backends = this.resources.Where(rr => rr.Value<string>("type") == "Microsoft.ApiManagement/service/backends");

            foreach (var backend in backends)
            {
                var name = GetParameterName(((string)backend["name"]), "_name");
                AddParameterFromObject((JObject)backend["properties"], "url", "string", name);

            }
        }

        public void ParameterizeAuthorizationServers()
        {
            var resources = this.resources.Where(rr => rr.Value<string>("type") == "Microsoft.ApiManagement/service/authorizationServers");

            foreach (var resource in resources)
            {
                var name = GetParameterName(((string)resource["name"]), "_name");
                AddParameterFromObject((JObject)resource["properties"], "clientRegistrationEndpoint", "string", name);
                AddParameterFromObject((JObject)resource["properties"], "authorizationEndpoint", "string", name);
                AddParameterFromObject((JObject)resource["properties"], "tokenEndpoint", "string", name);
                AddParameterFromObject((JObject)resource["properties"], "clientSecret", "securestring", name);
                AddParameterFromObject((JObject)resource["properties"], "resourceOwnerUsername", "string", name);
                AddParameterFromObject((JObject)resource["properties"], "resourceOwnerPassword", "string", name);

                //handle clientid                
                var orgclientidvalue = resource["properties"].Value<string>("clientId");
                var matches = System.Text.RegularExpressions.Regex.Match(orgclientidvalue, "'(?<val>[a-zA-Z0-9_-]*)'\\)\\]");
                if (matches.Groups.Count > 0)
                {
                    var orgvalue = matches.Groups["val"].Value;
                    var paramname = this.AddParameter(name + "_clientId", "securestring", orgvalue);

                    resource["properties"]["clientId"] = orgclientidvalue.Replace($"'{orgvalue}'", WrapParameterName(paramname));
                }

            }
        }


        /** PREVIEW FIXES **/
        public void FixOperationsMissingUrlTemplateParameters()
        {

            var resources = this.resources.Where(rr => rr.Value<string>("type") == "Microsoft.ApiManagement/service/apis/operations");

            foreach (var resource in resources)
            {
                string urlTemplate = resource["properties"].Value<string>("urlTemplate");
                JArray templateParameters = (JArray)resource["properties"]["templateParameters"];
                if (!string.IsNullOrEmpty(urlTemplate))
                {
                    var match = Regex.Match(urlTemplate, "{[a-zA-Z0-9_-]*}");
                    while (match.Success)
                    {
                        if (templateParameters == null)
                        {
                            templateParameters = new JArray();
                            resource["properties"]["templateParameters"] = templateParameters;
                        }
                        var name = match.Value.Replace("{", "").Replace("}", "");
                        var obj = new JObject();
                        obj.Add("name", name);
                        obj.Add("type", "string");
                        obj.Add("required", true);
                        templateParameters.Add(obj);
                        match = match.NextMatch();
                    }
                }
            }
        }
    }
}
