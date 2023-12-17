using DataAccess.Dtos;
using DataAccess.Entities;

namespace API.Contracts;

public record PostUpsertRequest(
    string? Id,
    string Title,
    string Body,
    string Tags, // string separeated by comma, first trimm it the split on comma the get into a comma !
    string Categories,
    bool IsPublised = false
);

public record PostReactionRequest(
        string PostId,
        ReactionType ReactionType
);