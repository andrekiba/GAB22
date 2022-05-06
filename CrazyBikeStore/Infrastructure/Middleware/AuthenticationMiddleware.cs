using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using CrazyBikeStore.Infrastructure.Auth;
using CrazyBikeStore.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace CrazyBikeStore.Infrastructure.Middleware
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        readonly JwtSecurityTokenHandler tokenValidator;
        readonly TokenValidationParameters tokenValidationParameters;
        readonly ConfigurationManager<OpenIdConnectConfiguration> configurationManager;
        readonly IConfiguration configuration;

        public AuthenticationMiddleware(IConfiguration configuration)
        {
            this.configuration = configuration;
            var authority = configuration["AzureAD:Authority"];
            var audience = configuration["AzureAD:Audience"];
            tokenValidator = new JwtSecurityTokenHandler();
            tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = audience
            };
            configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
        }
        
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            //skip swagger functions
            if (context.FunctionDefinition.EntryPoint.Contains("Microsoft.Azure.Functions.Worker.Extensions.OpenApi"))
                await next(context);
            else
            {
                var authorizeAttributes = context.GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>();
                if (authorizeAttributes.Any())
                {
                    if (!TryGetTokenFromHeaders(context, out var token))
                    {
                        // Unable to get token from headers
                        context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                        return;
                    }

                    if (!tokenValidator.CanReadToken(token))
                    {
                        // Token is malformed
                        context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                        return;
                    }

                    // Get OpenID Connect metadata
                    var validationParameters = tokenValidationParameters.Clone();
                    var openIdConfig = await configurationManager.GetConfigurationAsync(default);
                    validationParameters.ValidIssuers = new List<string> { openIdConfig.Issuer };
                    validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

                    try
                    {
                        // Validate token
                        var principal = tokenValidator.ValidateToken(
                            token, validationParameters, out _);

                        // Set principal + token in Features collection
                        // They can be accessed from here later in the call chain
                        context.Features.Set(new JwtPrincipalFeature(principal, token));
                    }
                    catch (SecurityTokenException)
                    {
                        // Token is not valid (expired etc.)
                        context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                    }
                }

                await next(context);
            }
        }
        
        static bool TryGetTokenFromHeaders(FunctionContext context, out string token)
        {
            token = null;
            // HTTP headers are in the binding context as a JSON object
            // The first checks ensure that we have the JSON string
            if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
            {
                return false;
            }

            if (headersObj is not string headersStr)
            {
                return false;
            }

            // Deserialize headers from JSON
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersStr);
            var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            if (!normalizedKeyHeaders.TryGetValue("authorization", out var authHeaderValue))
            {
                // No Authorization header present
                return false;
            }

            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Scheme is not Bearer
                return false;
            }

            token = authHeaderValue.Substring("Bearer ".Length).Trim();
            return true;
        }
    }
}