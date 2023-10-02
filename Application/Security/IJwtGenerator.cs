using Domain.Entities;

namespace Application.Security;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}