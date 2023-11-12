using DataAccess.Dtos;
using DataAccess.Shared;

namespace DataAccess.Repos
{
    public interface IPostRepo
    {
        Task<Result<PostReadDetailsDto>> CreatePost(PostUpsertDto postDto);
        Task<Result<bool>> HardDelete(Guid postId);
        Task<Result<PagedList<PostGeneralInfoDto>>> GetAll(
         int page = 1,
         int pageSize = 10,
         string? searchTerm = null,
         string sortBy = "publish_date",
         bool isSortAscending = true,
         string? filterValue = null,
         string? filterType = null);
        Task<Result<Dictionary<string, List<PostGeneralInfoDto>>>> GetAllPostsGroupedByCategory();
        Task<Result<List<ArchivePostDto>>> GetArchivePosts();
        Task<Result<PostReadDetailsDto>> GetBySlug(string slug);
        Task<Result<PostReadDetailsDto>> UpsertPost(string authorId, Guid? postId, PostUpsertDto postDto);
        Task<Result<bool>> SoftDelete(Guid postId);
    }
}