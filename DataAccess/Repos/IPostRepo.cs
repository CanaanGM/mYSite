using DataAccess.Dtos;

namespace DataAccess.Repos
{
    public interface IPostRepo
    {
        Task<Result<PostReadDto>> CreatePost(PostUpsertDto postDto);
        Task<Result<bool>> Delete(Guid postId);
        Task<Result<List<PostReadDto>>> GetAll();
        Task<Result<PostReadDto>> GetBySlug(string slug);
        Task<Result<PostReadDto>> UpsertPost(Guid postId, PostUpsertDto postDto);
    }
}