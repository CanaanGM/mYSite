using DataAccess.Dtos;
using DataAccess.Entities;

namespace API.Contracts;

public record PostUpsertRequest(
    Guid? Id,
    string Title,
    string Body,
    List<TagUpsertDto> Tags,
    List<CategoryUpsertDto> Categories,
    bool IsPublised = false
);

public record PostReactionRequest(
        string PostId,
        ReactionType ReactionType
);