using System.Security.Claims;

namespace CrazyBikeStore.Infrastructure.Auth
{
    public class JwtPrincipalFeature
    {
        public ClaimsPrincipal Principal { get; }
        public string AccessToken { get; }
        public JwtPrincipalFeature(ClaimsPrincipal principal, string accessToken)
        {
            Principal = principal;
            AccessToken = accessToken;
        }
    }
}