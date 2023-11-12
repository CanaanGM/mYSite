using DataAccess.Dtos;

namespace API.Contracts;

public record PostUpsertRequest(
    Guid? Id,
    string Title,
    string Body,
    List<TagUpsertDto> Tags,
    List<CategoryUpsertDto> Categories,
    bool IsPublised = false
);