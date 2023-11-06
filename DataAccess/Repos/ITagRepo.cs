using DataAccess.Dtos;

using Domain.Entities;

namespace DataAccess.Repos
{
    public interface ITagRepo
    {
        Task<Result<List<TagReadDto>>> GetAllTags();
        Task<Result<Tag>> GetOrCreateTagAsync(string tagName);
        Task<Result<TagReadDto>> GetTagById(string name);
    }
}