// --------------------------------------------------------------------------
//  <copyright file="StringExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    static class StringExtensions
    {
        public static string ToLowerFirstChar(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return $"{char.ToLower(str[0])}{str.Substring(1)}";
        }

        public static string ToUpperFirstChar(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return $"{char.ToUpper(str[0])}{str.Substring(1)}";
        }
    }
}
