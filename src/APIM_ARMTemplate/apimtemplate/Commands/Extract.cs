using System;
using System.IO;
using System.Net;
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

            var apiManagementName = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue);
            var resourceGroupName = this.Option("--resourceGroup <resourceGroup>", "Resource Group name", CommandOptionType.SingleValue);

            this.HelpOption();
                 
            this.OnExecute(() =>
            {
                if (!apiManagementName.HasValue()) throw new Exception("Missing parameter <apimname>.");
                if (!resourceGroupName.HasValue()) throw new Exception("Missing parameter <resourceGroup>.");

               // TemplateCreator templateCreator = new TemplateCreator();
                
                string resourceGroup = resourceGroupName.Values[0].ToString();
                string apimname = apiManagementName.Values[0].ToString();
                Api api = new Api();
                string apis = api.GetAPIs(apimname, resourceGroup).Result;
                JObject oApis = JObject.Parse(apis);
                int count = oApis["value"].Count<object>();

                ConsoleColor lastColor = Console.ForegroundColor; 
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("{0} API's found!", count);
                Console.ForegroundColor = lastColor;

                DownloadFile("https://github.com/odaibert/apim_templates/blob/master/API.json", "API.json");

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
        public static void DownloadFile(string sourceURL, string destinationPath)
        {
            long fileSize = 0;
            int bufferSize = 1024;
            bufferSize *= 1000;
            long existLen = 0;
            if (File.Exists(destinationPath))
            {
                FileInfo destinationFileInfo = new FileInfo(destinationPath);
                existLen = destinationFileInfo.Length;
            }


            FileStream saveFileStream;
            if (existLen > 0)
                saveFileStream = new FileStream(destinationPath,
                                                          FileMode.Append,
                                                          FileAccess.Write,
                                                          FileShare.ReadWrite);
            else
                saveFileStream = new FileStream(destinationPath,
                                                          FileMode.Create,
                                                          FileAccess.Write,
                                                          FileShare.ReadWrite);

            HttpWebRequest httpReq;
            HttpWebResponse httpRes;
            httpReq = (HttpWebRequest)WebRequest.Create(sourceURL);
            httpReq.AddRange((int)existLen);
            Stream resStream;
            httpRes = (HttpWebResponse)httpReq.GetResponse();
            resStream = httpRes.GetResponseStream();

            fileSize = httpRes.ContentLength;

            int byteSize;
            byte[] downBuffer = new byte[bufferSize];

            while ((byteSize = resStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
            {
                saveFileStream.Write(downBuffer, 0, byteSize);
            }
        }

    }
}