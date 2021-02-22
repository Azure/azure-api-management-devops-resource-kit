using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create;
using System.Collections.Generic;
using System.Linq;

namespace apimtemplate.Creator.Utilities
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
            CreatorConfig creatorConfig, CommandOption namedValuesInstance)
        {
            if (namedValuesInstance.HasValue() && !string.IsNullOrEmpty(namedValuesInstance.Value()))
            {
                string inputNamedInstances = namedValuesInstance.Value();
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
                                creatorConfig.namedValues.Where(x => x.displayName == keyValueSeperatedArray[0]).FirstOrDefault().value = keyValueSeperatedArray[1];
                            }
                        }
                    }


                }
            }
        }
    }
}