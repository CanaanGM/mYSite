namespace API.Contracts;

public record UserLoginRequest(
    string Email,
    string Password
);

public record UserRegisterRequest(
    string Email,
    string UserName,
    string Password,
    string? profilePicture

);

public record UserNameCheckRequest(string Username);
public record EmailCheckRequest(string Email);
public record ConfirmEmailRequest(string Email);
public record PasswordResetRequest(string Verifier);
public record PasswordChangeRequest(string newPassword);