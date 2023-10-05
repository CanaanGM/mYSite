using DataAccess.Dtos;

namespace API.Contracts;

public record PostUpsertRequest(
    string Title,
    string Body,
    List<TagUpsertDto> Tags,
    List<CategoryUpsertDto> Categories,
    bool IsPublised = false
);