// Ignore Spelling: Repo Repos

using DataAccess.Dtos;
using DataAccess.Shared;

namespace DataAccess.Repos
{
    public interface ICommentRepo
    {
        Task<Result<IEnumerable<CommentReadDto>>> GetCommentsForPost(Guid postId);
        Task<Result<bool>> SoftDeleteComment(Guid commentId, string authorId);
        Task<Result<CommentReadDto>> UpsertComment(CommentCreateDto upsertComment, string authorId);
    }
}