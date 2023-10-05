using DataAccess.Entities;

namespace Application.Security;

public interface IJwtGenerator
{
    string GenerateToken(User user, List<string> userRoles);
}