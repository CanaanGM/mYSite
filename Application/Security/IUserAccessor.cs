// Ignore Spelling: Accessor Username

namespace Application.Security
{
    public interface IUserAccessor
    {
        string? GetUserId();

        string? GetUsername();
    }
}