// Ignore Spelling: Repo Repos

using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Shared;

namespace DataAccess.Repos
{
    public interface ICommentRepo
    {
        Task<Result<PagedList<CommentReadForAuthorDto>>> GetAllCommentsForUser(string userId,
            int page = 1,
            int pageSize = 10);

        Task<Result<IEnumerable<CommentReadDto>>> GetCommentsForPost(Guid postId);

        Task<Result<bool>> SoftDeleteComment(Guid commentId, string authorId);

        Task<Result<CommentReadDto>> UpsertComment(CommentCreateDto upsertComment, string authorId);

        Task<Result<bool>> UpsertCommentReaction(string userId, Guid commentId, ReactionType reactionType);
    }
}