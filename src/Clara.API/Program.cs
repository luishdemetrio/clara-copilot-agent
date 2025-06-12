using Azure.Identity;
using Lance.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));



// Add configuration for Azure AD to be used by Graph
var config = builder.Configuration;
builder.Services.AddSingleton(new ClientSecretCredential(
                     config["AzureAd:TenantId"],
                     config["AzureAd:ClientId"],
                     config["AzureAd:ClientSecret"]
                    ));

builder.Services.AddSingleton<GraphServiceClient>(sp =>
{
    var credential = sp.GetRequiredService<ClientSecretCredential>();
    return new GraphServiceClient(credential);
});


// Add services to the container.
builder.Services.AddControllers();


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{config["AzureAd:Instance"]}{config["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{config["AzureAd:Instance"]}{config["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>{
                        { $"api://{config["AzureAd:ClientId"]}/access_as_user", "Access API" }
                     }
            }
        }
    });
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllers();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'self' teams.microsoft.com *.teams.microsoft.com *.skype.com; script-src 'self' *.microsoft.com *.microsoftonline.com;");
    await next();
});

app.Run();
