using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace CrazyBikeStore.Infrastructure.AuthFlows;

public class BikeStoreAuth : OpenApiOAuthSecurityFlows
{
    public BikeStoreAuth()
    {
        Implicit = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri("http://petstore.swagger.io/oauth/dialog"),
            Scopes = { { "write:bikes", "modify bikes in your account" }, { "read:bikes", "read your bikes" } }
        };
    }
}