namespace apimtemplate.Extractor.Models
{
    public class serviceUrlProperty
    {
        public string apiName { get; private set; }
        public string serviceUrl { get; private set; }
        public serviceUrlProperty(string apiName, string serviceUrl)
        {
            this.apiName = apiName;
            this.serviceUrl = serviceUrl;
        }
    }
}
