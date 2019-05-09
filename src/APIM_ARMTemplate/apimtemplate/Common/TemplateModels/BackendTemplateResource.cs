
namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    public class BackendTemplateResource : TemplateResource
    {
        public BackendTemplateProperties properties { get; set; }
    }

    public class BackendTemplateProperties
    {
        public string title { get; set; }
        public string description { get; set; }
        public string resourceId { get; set; }
        public BackendSubProperties properties { get; set; }
        public BackendCredentials credentials { get; set; }
        public BackendProxy proxy { get; set; }
        public BackendTLS tls { get; set; }
        public string url { get; set; }
        public string protocol { get; set; }
    }

    public class BackendSubProperties
    {
        public BackendServiceFabricCluster serviceFabricCluster { get; set; }
    }

    public class BackendServiceFabricCluster
    {
        public string clientCertificatethumbprint { get; set; }
        public int maxPartitionResolutionRetries { get; set; }
        public string[] managementEndpoints { get; set; }
        public string[] serverCertificateThumbprints { get; set; }
        public ServerX509Names[] serverX509Names { get; set; }
    }

    public class ServerX509Names
    {
        public string name { get; set; }
        public string issuerCertificateThumbprint { get; set; }
    }

    public class BackendCredentials
    {
        public string[] certificate { get; set; }
        public object query { get; set; }
        public object header { get; set; }
        public BackendCredentialsAuthorization authorization { get; set; }
    }

    public class BackendCredentialsAuthorization
    {
        public string scheme { get; set; }
        public string parameter { get; set; }
    }

    public class BackendProxy
    {
        public string url { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public class BackendTLS
    {
        public bool validateCertificateChain { get; set; }
        public bool validateCertificateName { get; set; }
    }
}
