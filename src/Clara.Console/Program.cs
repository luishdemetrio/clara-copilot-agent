using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;


class Program
{
    private static async Task Main(string[] args)
    {
        string apiUrl = "";
        string tenantId = "b5d31b4e-6d83-4373-b61b-de1b0cd6f140";
        string clientId = "755ad9c9-ff4f-4c32-b412-dbd20568d46e";
        string clientSecret = "B3Y8Q~xpHidbDsnz39sElhAp03nOYcfQBv8qFbsN";


        string[] scopes = { "api://28d4ccf2-99fb-47e2-ad49-58677a54c6f1/access_as_user" };
        var app = PublicClientApplicationBuilder
            .Create("a262e8da-b82f-4111-b34a-eea53ae15632")
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .WithRedirectUri("http://localhost")
                .Build();

        //// Step 1: Acquire an Access Token
        var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

        await CallApi("https://valued-weasel-stirring.ngrok-free.app/api/Copilot/usage-report", result.AccessToken);


        //// Step 1: Acquire an Access Token
        string accessToken = await GetAccessToken(tenantId, clientId, clientSecret,
                "api://28d4ccf2-99fb-47e2-ad49-58677a54c6f1/.default");

        await CallApi("https://valued-weasel-stirring.ngrok-free.app/api/Copilot/usage-report", accessToken);

    }  


    private static async Task<string> GetAccessToken(string tenantId, string clientId, string clientSecret, 
                                                     string apiScope)
    {
        // Create a confidential client application
        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .Build();

        // Acquire a token for the specified scope
        var result = await app.AcquireTokenForClient(new[] { apiScope }).ExecuteAsync();
        Console.WriteLine($"Access Token Acquired: {result.AccessToken.Substring(0, 50)}..."); // Log a portion of the token
        return result.AccessToken;
    }


    private static async Task CallApi(string apiUrl, string accessToken)
    {
        // Create an HttpClient to make the request
        using var httpClient = new HttpClient();

        // Add the access token to the Authorization header
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Send the GET request
        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            string responseData = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response:");
            Console.WriteLine(responseData);
        }
        else
        {
            Console.WriteLine($"API call failed with status code: {response.StatusCode}");
            Console.WriteLine($"Error: {await response.Content.ReadAsStringAsync()}");
        }
    }
}