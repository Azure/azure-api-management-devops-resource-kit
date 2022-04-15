// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using System.IO;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Tests.Extractor.Abstractions
{
    public class ExtractorMockerWithOutputTestsBase : ExtractorMockerTestsBase
    {
        protected const string TESTS_OUTPUT_DIRECTORY = "tests-output";
        protected string OutputDirectory;

        protected ExtractorMockerWithOutputTestsBase(string outputDirectoryName)
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
