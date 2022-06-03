// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions
{
    public abstract class CreatorMockerWithOutputTestsBase : TestsBase
    {
        protected const string TESTS_OUTPUT_DIRECTORY = "tests-output\\creator";
        protected string OutputDirectory;
        
        protected CreatorMockerWithOutputTestsBase(string outputDirectoryName)
        {
            this.OutputDirectory = Path.Combine(TESTS_OUTPUT_DIRECTORY, outputDirectoryName);

            // remember to clean up the output directory before each test
            if (Directory.Exists(this.OutputDirectory))
            {
                Directory.Delete(this.OutputDirectory, true);
            }
        }
    }
}
