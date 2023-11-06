
using Application.Dtos;

namespace API.Contracts;

public record PostResponse<T>(
    string Status,
    T Value
);


public record GroupedPostsResponse(
    Dictionary<string, PostReadDto> Posts
    );