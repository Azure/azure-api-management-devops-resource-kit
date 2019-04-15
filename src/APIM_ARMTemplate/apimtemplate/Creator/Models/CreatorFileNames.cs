
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class CreatorFileNames
    {
        public string apiVersionSet { get; set; }
        public string initialAPI { get; set; }
        public string subsequentAPI { get; set; }
        // linked property outputs 1 master template
        public string linkedMaster { get; set; }
        // unlined property outputs 2 master templates
        public string unlinkedMasterOne { get; set; }
        public string unlinkedMasterTwo { get; set; }
        public string masterParameters { get; set; }
    }
}
