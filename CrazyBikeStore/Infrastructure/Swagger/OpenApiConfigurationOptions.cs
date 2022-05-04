using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace CrazyBikeStore.Infrastructure.Swagger;

public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiVersionType OpenApiVersion { get; set; } = GetOpenApiVersion();
    
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = GetOpenApiDocVersion(),
        Title = GetOpenApiDocTitle(),
        Description = "CrazyBike Store API that runs on Azure Functions",
        Contact = new OpenApiContact
        {
            Name = "CrazyBike Store",
            Email = "info@elfo.net",
            Url = new Uri("https://www.elfo.net")
        },
        // License = new OpenApiLicense
        // {
        //     
        // },
        //TermsOfService = new Uri("")
    };
}