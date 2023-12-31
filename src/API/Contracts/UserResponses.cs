using DataAccess.Dtos;
using DataAccess.Shared;

namespace API.Contracts;

public record UserNameCheckResponse(string message, bool isAvailable);
public record EmailCheckResponse(string message, bool isAvailable);
public record CurrentUserResponse(
    string Email,
    string UserName,
    string[] Roles
    );
public record LoginResponse(
    string id,
    string Email,
    string UserName,
    string Token,
    string[] Roles
    );

public record EmailConfirmationResponse(string message);
public record RegisterResponse(string message);

public record UserProfileResponse(
    string username,
    string email,
    string[] Roles,
    string? profilePicture,
    PagedList<CommentReadForAuthorDto> comments,
    PagedList<PostReadWithEntity> favoritePosts
    );