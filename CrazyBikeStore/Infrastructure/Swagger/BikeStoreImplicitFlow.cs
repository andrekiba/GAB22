using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace CrazyBikeStore.Infrastructure.Swagger;

public class BikeStoreImplicitFlow : OpenApiOAuthSecurityFlows
{
    const string AuthUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize";
    const string RefreshUrl = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
    public BikeStoreImplicitFlow()
    {
        var tenantId = Environment.GetEnvironmentVariable("OpenApi__Auth__TenantId");
        var audience = Environment.GetEnvironmentVariable("OpenApi__Auth__Audience");
        
        Implicit = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri(string.Format(AuthUrl, tenantId)),
            RefreshUrl = new Uri(string.Format(RefreshUrl, tenantId)),
            Scopes =
            {
                //{"https://graph.microsoft.com/.default", "Default scope defined in the app"},
                {$"api://{audience}/Bikes.Read","Bikes.Read"},
                {$"api://{audience}/Bikes.Write","Bikes.Write"}
            }
        };
    }
}