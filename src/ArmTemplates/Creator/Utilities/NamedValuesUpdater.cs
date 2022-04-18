// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Models;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Creator.Utilities
{
    public class NamedValuesUpdater
    {
        /// <summary>
        /// Seperate multiple named instances using semicolon
        /// </summary>
        public static readonly char MultiKeySeperator = ';';

        /// <summary>
        /// Seperate named key value pair using underscore
        /// </summary>
        public static readonly char KeyValueSeperator = '|';

        public void UpdateNamedValueInstances(
            CreatorConfig creatorConfig, string namedValuesInstance)
        {
            if (!string.IsNullOrEmpty(namedValuesInstance))
            {
                string inputNamedInstances = namedValuesInstance;
                // Validation to see number of underscores match number of semicolons
                if (inputNamedInstances.Count(f => f == MultiKeySeperator) == inputNamedInstances.Count(f => f == KeyValueSeperator))
                {
                    // Splint based on semicolon
                    string[] namedValues = inputNamedInstances.Split(MultiKeySeperator);

                    foreach (string keyValue in namedValues)
                    {
                        if (keyValue.Contains(KeyValueSeperator))
                        {
                            string[] keyValueSeperatedArray = keyValue.Split(KeyValueSeperator);
                            if (creatorConfig.namedValues != null && creatorConfig.namedValues.Count > 0)
                            {
                                creatorConfig.namedValues.Where(x => x.DisplayName == keyValueSeperatedArray[0]).FirstOrDefault().Value = keyValueSeperatedArray[1];
                            }
                        }
                    }


                }
            }
        }
    }
}