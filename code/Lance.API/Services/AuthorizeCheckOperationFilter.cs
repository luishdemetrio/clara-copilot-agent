namespace Lance.API.Services;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                              .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any()
                          || context.MethodInfo.GetCustomAttributes(true)
                              .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security = new System.Collections.Generic.List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [ new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        }
                    ] = new[] { "api://<your-client-id>/.default" }
                }
            };
        }
    }
}
