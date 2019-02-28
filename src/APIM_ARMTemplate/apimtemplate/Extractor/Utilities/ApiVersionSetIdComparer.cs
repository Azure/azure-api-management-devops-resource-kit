using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extract
{
    public class ApiVersionSetIdComparer : IEqualityComparer<JToken>
    {
        private const string Properties = "properties";
        private const string ApiVersionSetId = "apiVersionSetId";

        public bool Equals(JToken x, JToken y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            if (x[Properties] == null || y[Properties] == null)
                return false;
            return x[Properties].Value<string>(ApiVersionSetId)
                .Equals(y[Properties].Value<string>(ApiVersionSetId));
        }

        public int GetHashCode(JToken obj)
        {
            if (obj[Properties] == null)
                return obj.GetHashCode();
            return (obj[Properties].Value<string>(ApiVersionSetId) ?? String.Empty).GetHashCode();
        }
    }
}