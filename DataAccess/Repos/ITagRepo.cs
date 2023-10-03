using Domain.Entities;

namespace DataAccess.Repos
{
    public interface ITagRepo
    {
        Task<Result<Tag>> GetOrCreateTagAsync(string tagName);
    }
}