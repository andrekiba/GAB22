using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using CrazyBikeStore.Infrastructure.Auth;
using CrazyBikeStore.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CrazyBikeStore.Infrastructure.Middleware
{
    public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        readonly IAuthorizationService authorizationService;
        public AuthorizationMiddleware(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }
        
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            //skip swagger functions
            if (context.FunctionDefinition.EntryPoint.Contains("Microsoft.Azure.Functions.Worker.Extensions.OpenApi"))
                await next(context);
            else
            {
                var principalFeature = context.Features.Get<JwtPrincipalFeature>();
                var targetMethod = context.GetTargetFunctionMethod();
                var authorizeAttributes = GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(targetMethod);
                
                if (authorizeAttributes.Any())
                {
                    foreach (var authorizeAttribute in authorizeAttributes)
                    {
                        //[Authorize]
                        if (string.IsNullOrEmpty(authorizeAttribute.Policy) && string.IsNullOrEmpty(authorizeAttribute.Roles))
                        {
                            if (!await AuthorizedForAuthenticated(principalFeature?.Principal))
                            {
                                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                                return;
                            }
                        }

                        //[Authorize(Roles="Admin")]
                        if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
                        {
                            if (principalFeature?.Principal != null)
                            {
                                if (!await AuthorizedForRoles(authorizeAttribute?.Roles, principalFeature?.Principal))
                                {
                                    context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);
                                    return;
                                }
                            }
                            else
                            {
                                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                                return;
                            }
                        }   

                        //[Authorize("MyCustomPolicy")]
                        //[Authorize(Policy="MyCustomPolicy")]
                        if (!string.IsNullOrEmpty(authorizeAttribute.Policy))
                        {
                            if (principalFeature?.Principal != null)
                            {
                                if (!await AuthorizedForPolicy(authorizeAttribute.Policy, principalFeature?.Principal))
                                {
                                    context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);
                                    return;
                                }
                            }
                            else
                            {
                                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                                return;
                            }
                        }
                    }
                }

                await next(context);
            }
        }
        
        async Task<bool> AuthorizedForAuthenticated(ClaimsPrincipal claimsPrincipal, List<string> authenticationSchemes = null)
        {
            if (claimsPrincipal == null)
                return false;

            var rolesResult = await authorizationService.AuthorizeAsync(claimsPrincipal, RequireAuthenticatedUserPolicy(authenticationSchemes));
            return rolesResult.Succeeded;
        }
        async Task<bool> AuthorizedForRoles(string rolesString, ClaimsPrincipal claimsPrincipal, List<string> authenticationSchemes = null)
        {
            if (string.IsNullOrEmpty(rolesString))
                throw new ArgumentNullException(nameof(rolesString));

            if (claimsPrincipal == null)
                return false;
            
            var rolesResult = await authorizationService.AuthorizeAsync(claimsPrincipal, RequireRoles(rolesString.Split(","), authenticationSchemes));
            return rolesResult.Succeeded;
        }
        async Task<bool> AuthorizedForPolicy(string policy, ClaimsPrincipal claimsPrincipal)
        {
            if (string.IsNullOrEmpty(policy))
                throw new ArgumentNullException(nameof(policy));

            if (claimsPrincipal == null)
                return false;

            var rolesResult = await authorizationService.AuthorizeAsync(claimsPrincipal, policy);
            return rolesResult.Succeeded;
        }
        
        static AuthorizationPolicy RequireRoles(string[] roles, IList<string> authenticationSchemes = null)
        {
            var authPolicyBuilder = new AuthorizationPolicyBuilder().RequireRole(roles);
            if (authenticationSchemes != null) authPolicyBuilder.AuthenticationSchemes = authenticationSchemes;
            return authPolicyBuilder.Build();
        }
        static AuthorizationPolicy RequireAuthenticatedUserPolicy(IList<string> authenticationSchemes = null)
        {
            var authPolicyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            if (authenticationSchemes != null) authPolicyBuilder.AuthenticationSchemes = authenticationSchemes;
            return authPolicyBuilder.Build();
        }
        
        static List<T> GetCustomAttributesOnClassAndMethod<T>(MethodInfo targetMethod) where T : Attribute
        {
            var methodAttributes = targetMethod.GetCustomAttributes<T>();
            var classAttributes = targetMethod.DeclaringType.GetCustomAttributes<T>();
            var mAttributes = methodAttributes as T[] ?? methodAttributes.ToArray();
            return mAttributes.Any() ? mAttributes.ToList() : classAttributes.ToList();
        }
    }
}