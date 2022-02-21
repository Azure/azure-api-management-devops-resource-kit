// --------------------------------------------------------------------------
//  <copyright file="JsonExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    static class JsonExtensions
    {
        static readonly JsonSerializerSettings DefaultJsonSerializationSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string Serialize<T>(this T item) => JsonConvert.SerializeObject(item, DefaultJsonSerializationSettings);

        public static T Deserialize<T>(this string serializedItem) => JsonConvert.DeserializeObject<T>(serializedItem, DefaultJsonSerializationSettings);
    }
}
