using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ServerSide.Authentication
{
    public class NameTokenValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken { get; } = true;
        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            validatedToken = null;

            string name = securityToken;

            return new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, name)
                })
            });
        }
    }
}
