namespace Lance.API.Services;


using Azure.Identity;
using Microsoft.Graph;

public class GraphServiceClientFactory
{
    private readonly IConfiguration _config;
    public GraphServiceClientFactory(IConfiguration config) => _config = config;

    public GraphServiceClient Create()
    {
        var clientId = _config["AzureAd:ClientId"];
        var tenantId = _config["AzureAd:TenantId"];
        var clientSecret = _config["AzureAd:ClientSecret"];

        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        return new GraphServiceClient(clientSecretCredential);
    }
}
