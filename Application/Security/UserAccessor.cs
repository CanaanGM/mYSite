// Ignore Spelling: Accessor Username

using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
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
