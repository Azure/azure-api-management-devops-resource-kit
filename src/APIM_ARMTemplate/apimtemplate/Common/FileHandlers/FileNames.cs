namespace apimtemplate.Common.FileHandlers
{
    public class FileNames
    {
        public string apiVersionSets { get; set; }
        public string authorizationServers { get; set; }
        public string backends { get; set; }
        public string globalServicePolicy { get; set; }
        public string loggers { get; set; }
        public string namedValues { get; set; }
        public string tags { get; set; }
        public string products { get; set; }
        public string productAPIs { get; set; }
        public string apiTags { get; set; }
        public string parameters { get; set; }
        // linked property outputs 1 master template
        public string linkedMaster { get; set; }
        public string apis { get; set; }
        public string splitAPIs { get; set; }
        public string versionSetMasterFolder { get; set; }
        public string revisionMasterFolder { get; set; }
        public string groupAPIsMasterFolder { get; set; }
        public string baseFileName { get; set; }
    }
}
