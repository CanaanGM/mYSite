namespace API.Contracts;

public record UserLoginRequest(
    string Email,
    string Password
);

public record UserRegisterRequest(
    string Email,
    string UserName,
    string Password

);

public record UserNameCheckRequest(string Username);
public record EmailCheckRequest(string Email);