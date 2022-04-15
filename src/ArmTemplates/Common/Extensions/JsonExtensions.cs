// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
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

        public static T Deserialize<T>(this string serializedItem, JsonConverter jsonConverter) => JsonConvert.DeserializeObject<T>(serializedItem, converters: jsonConverter);
    }
}
