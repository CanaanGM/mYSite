using DataAccess.Entities;

namespace API.Contracts
{

    public record CommentReactionRequest(
            Guid commentId,
            ReactionType reactionType
    );

}
