using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace CrazyBikeStore.Infrastructure.Swagger;

public class BikeStoreAuthorizationCodeFlow : OpenApiOAuthSecurityFlows
{
    const string AuthUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize";
    const string TokenUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
    const string RefreshUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
    public BikeStoreAuthorizationCodeFlow()
    {
        var tenantId = Environment.GetEnvironmentVariable("OpenApi__Auth__TenantId");
        var audience = Environment.GetEnvironmentVariable("OpenApi__Auth__Audience");
        
        AuthorizationCode = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri(string.Format(AuthUrl, tenantId)),
            TokenUrl = new Uri(string.Format(TokenUrl, tenantId)),
            RefreshUrl = new Uri(string.Format(RefreshUrl, tenantId)),
            Scopes =
            {
                {$"api://{audience}/Bikes.Read","Bikes.Read"},
                {$"api://{audience}/Bikes.Write","Bikes.Write"}
            }
        };
    }
}