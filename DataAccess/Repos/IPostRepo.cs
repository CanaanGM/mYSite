using DataAccess.Dtos;
using Domain.Shared;

namespace DataAccess.Repos
{
    public interface IPostRepo
    {
        Task<Result<PostReadDto>> CreatePost(PostUpsertDto postDto);
        Task<Result<bool>> Delete(Guid postId);
        Task<Result<PagedList<PostReadDto>>> GetAll(
         int page = 1,
         int pageSize = 10,
         string? searchTerm = null,
         string sortBy = "publish_date",
         bool isSortAscending = true,
         string? filterValue = null,
         string? filterType = null);
        Task<Result<PostReadDto>> GetBySlug(string slug);
        Task<Result<PostReadDto>> UpsertPost(Guid postId, PostUpsertDto postDto);
    }
}