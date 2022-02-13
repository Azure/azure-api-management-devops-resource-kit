﻿using apimtemplate.Common.Templates.Abstractions;
using apimtemplate.Extractor.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apimtemplate.Extractor.EntityExtractors.Abstractions
{
    public interface IApiExtractor
    {
        Task<JToken[]> GetAllApiObjsAsync(string apiManagementName, string resourceGroupName);

        Task<List<string>> GetAllApiNamesAsync(string apiManagementName, string resourceGroupName);

        Task<string> GetServiceDiagnosticsAsync(string apiManagementName, string resourceGroupName);

        Task<string> GetApiDiagnosticsAsync(string apiManagementName, string resourceGroupName, string apiName);
        
        Task<Template> GenerateAPIsARMTemplateAsync(string singleApiName, List<string> multipleApiNames, ExtractorParameters extractorParameters);

        Task<string> GetAPIRevisionsAsync(string apiManagementName, string resourceGroupName, string apiName);

        Task<Template> GenerateAPIRevisionTemplateAsync(string currentRevision, List<string> revList, string apiName, ExtractorParameters exc);
    }
}
