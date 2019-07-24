using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace apimtemplate.Creator.OpenApiSpecEditors
{
    public interface IOpenApiDocumentProcessor
    {
        void Process(APIConfig config, OpenApiDocument specification);
    }

    public class BackendServiceSetter : IOpenApiDocumentProcessor
    {
        public void Process(APIConfig config, OpenApiDocument specification)
        {
            specification.Servers = new List<OpenApiServer>
            {
                new OpenApiServer{Url = (new Uri(config.serviceUrl, UriKind.Absolute)).AbsoluteUri}
            };
        }
    }

    public class OperationPathReplace : IOpenApiDocumentProcessor
    {
        public void Process(APIConfig config, OpenApiDocument specification)
        {
            if(string.IsNullOrEmpty(config.pathsCleanUp))
                return;

            var keys = new List<string>(specification.Paths.Keys);
            foreach (var key in keys)
            {
                if (key.Contains(config.pathsCleanUp))
                {
                    var temp = specification.Paths[key];
                    specification.Paths.Add(key.Replace(config.pathsCleanUp,"", StringComparison.OrdinalIgnoreCase), temp);
                    specification.Paths.Remove(key);
                }
            }
        }
    }

    public class OpenApiDocumentProcessor   
    {
        private readonly OpenApiDocument _document;
        private readonly APIConfig _config;

        private OpenApiDocumentProcessor(OpenApiDocument document, APIConfig config)
        {
            _document = document;
            _config = config;
        }

        public static OpenApiDocumentProcessor Load(APIConfig config)
        {
            OpenApiDocument document;
            using (var reader = File.OpenRead(config.openApiSpec))
            {
                document = new OpenApiStreamReader().Read(reader, out _);
            }
            return new OpenApiDocumentProcessor(document, config);
        }

        public static OpenApiDocumentProcessor Load(string fileContent, APIConfig config)
        {
            var reader = new OpenApiStringReader();
            var document = reader.Read(fileContent, out _);
            return new OpenApiDocumentProcessor(document, config);
        }

        public void Process(IOpenApiDocumentProcessor processor)
        {
            processor.Process(_config, _document);
        }
    }
}
