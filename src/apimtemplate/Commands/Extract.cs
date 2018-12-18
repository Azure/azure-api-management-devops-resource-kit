using System;
using McMaster.Extensions.CommandLineUtils;
using Colors.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class ExtractCommand : CommandLineApplication
    {
        public ExtractCommand()
        {
            this.Name = Constants.ExtractName;
            this.Description = Constants.ExtractDescription;

            var apimname = this.Option("--name <apimname>", "API Management name", CommandOptionType.SingleValue).IsRequired();

            var auth = new Authentication();

            var aztoken = auth.GetAccessToken().Result;

            this.HelpOption();

            this.OnExecute(async () =>
            {
                if (apimname.HasValue())
                {
                    Console.WriteLine($"Create command executed with name {apimname.Value()}");

                    string requestUrl = "https://management.azure.com/subscriptions/cabe3525-a754-4bf7-9af4-1a9746604527/resourceGroups/apim-extractor/providers/Microsoft.ApiManagement/service/apim-extractor/apis/odaibert-logicapp/products?api-version=2018-06-01-preview";
                                          
                    using (HttpClient httpClient = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", aztoken);

                        HttpResponseMessage response = await httpClient.SendAsync(request);

                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();

                        ColoredConsole.Write(responseBody.ToString());
                        Console.ReadKey();
                    }
                }
                else
                {
                    ColoredConsole.Error.WriteLine("API Management name passed in");
                }
                return 0;
            });

        }
    }
}
 