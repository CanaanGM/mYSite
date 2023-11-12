using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Shared;

namespace DataAccess.Repos
{
    public interface ITagRepo
    {
        Task<Result<List<TagReadDto>>> GetAllTags();
        Task<Result<Tag>> GetOrCreateTagAsync(string tagName);
        Task<Result<TagReadDto>> GetTagByName(string name);
    }
}