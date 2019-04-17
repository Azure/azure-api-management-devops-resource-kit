
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class CreatorFileNames
    {
        public string apiVersionSets { get; set; }
        public string products { get; set; }
        public string loggers { get; set; }
        // linked property outputs 1 master template
        public string linkedMaster { get; set; }
        public string linkedParameters { get; set; }
        public string unlinkedParameters { get; set; }
    }
}
