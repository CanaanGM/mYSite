
namespace API.Contracts;

public record PostResponse<T>(
    string Status,
    T Value
);

