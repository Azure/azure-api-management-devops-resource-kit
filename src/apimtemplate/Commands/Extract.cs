using System;
using McMaster.Extensions.CommandLineUtils;
using Colors.Net;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = Constants.ExtractName;
            this.Description = Constants.ExtractDescription;

            var apiManagementName = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue).IsRequired();

            this.HelpOption();
                 
            this.OnExecute(() =>
            {
                if (apiManagementName.HasValue()) throw new Exception("Missing parameter(s)."); //Validade if is better exception or not

                string resourceGroup = "APIM-Extractor"; //change to get from commandline parameter
                string apimname;
                string apis;
                int count;

                Api api = new Api();
                JObject oApis;

                apimname = apiManagementName.Values[0].ToString();
                apis = api.GetAPIs(apimname, resourceGroup).Result;
                oApis = JObject.Parse(apis);
                count = oApis["value"].Count<object>();

                Console.WriteLine("{0} API's found!", count);

                for (int i = 0; i < count; i++)
                {
                    Console.WriteLine(oApis);
                    string apiname = (string)oApis["value"][i]["name"];
                    ColoredConsole.WriteLine(apiname);
                    ColoredConsole.WriteLine(api.GetAPIOperations(apimname, resourceGroup, apiname).Result);
                }
                Console.ReadKey();

                return 0;
            });
        }
        private static string FormatJSON(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}