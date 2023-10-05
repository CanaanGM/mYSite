namespace API.Contracts;

public record UserNameCheckResponse(string message, bool isAvailable);
public record EmailCheckResponse(string message, bool isAvailable);
public record CurrentUserResponse(
    string Email,
    string UserName,
    string[] Roles
    );
public record LoginResponse(
    string Email,
    string UserName,
    string Token,
    string[] Roles
    );