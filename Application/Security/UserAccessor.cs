// Ignore Spelling: Accessor Username

using Microsoft.AspNetCore.Http;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Security
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUsername()
        {
            var userSubClaim = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userSubClaim != null)
                return userSubClaim;
            return null;
        }

        public string? GetUserId()
        {
            Claim? userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.NameId);

            if (userIdClaim == null)
            {
                return null; // Or you can return a 403 Forbidden status code
            }
            return userIdClaim.Value;
        }
    }
}