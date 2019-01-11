using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates
{
    public class FileNameGenerator
    {
        public CreatorFileNames GenerateCreatorFileNames()
        {
            return new CreatorFileNames()
            {
                apiVersionSet = $@"/versionset.template.json",
                api = $@"/api.template.json",
                master = @"/master.template.json"
            };
        }
    }
}
