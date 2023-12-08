using API.Contracts;

using Application.Security;

using DataAccess.Entities;
using DataAccess.Repos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("/api/auth")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUserAccessor _userAccessor;
    private readonly ICommentRepo _commentRepo;
    private readonly IPostRepo _postRepo;

    public AccountController(
        UserManager<User> userManager,
        IJwtGenerator jwtGenerator,
        IUserAccessor userAccessor,
        ICommentRepo commentRepo,
        IPostRepo postRepo

        )
    {
        _jwtGenerator = jwtGenerator;
        _userAccessor = userAccessor;
        _commentRepo = commentRepo;
        _postRepo = postRepo;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterRequest userCreds)
    {
        var user = new User
        {
            Email = userCreds.Email,
            UserName = userCreds.UserName,
            EmailConfirmed = false,
        };

        var res = await _userManager.CreateAsync(user, userCreds.Password);

        if (!res.Succeeded)
        {
            foreach (var error in res.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return ValidationProblem();
        }

        await _userManager.AddToRoleAsync(user, "User");

        // TODO: email body generation
        var emailConfirmToken = Uri.EscapeDataString(await _userManager.GenerateEmailConfirmationTokenAsync(user));
        string confirmationLink = GenerateConfirmationLink(user, emailConfirmToken, nameof(ConfirmEmail));

        return CreatedAtAction(nameof(Register), new RegisterResponse(confirmationLink));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest userCreds)
    {
        var user = await _userManager.FindByEmailAsync(userCreds.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, userCreds.Password))
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);
        var token = _jwtGenerator.GenerateToken(user, userRoles.ToList());

        return Ok(
            new LoginResponse(
                user.Id,
                user.Email!,
                user.UserName!,
                token,
                userRoles.ToArray()
            ));
    }

    // todo
    // TODO: Think about this more carefully, it maybe a threat
    // password Rest request
    [AllowAnonymous]
    [HttpPost("requestPasswordReset")]
    public async Task<IActionResult> PasswordResetRequest(PasswordResetRequest resetRequest)
    {
        if (String.IsNullOrEmpty(resetRequest.Verifier))
            return BadRequest("provider a valid verifier");

        var user = await _userManager.FindByEmailAsync(resetRequest.Verifier)
            ?? await _userManager.FindByNameAsync(resetRequest.Verifier);

        if (user is null) return BadRequest("verifier is incorrect, please double check and try again");

        var passwordResetToken = Uri.EscapeDataString(await _userManager.GeneratePasswordResetTokenAsync(user));
        var confirmationLink = GenerateConfirmationLink(user, passwordResetToken, nameof(PasswordReset));

        // TODO: send as email yo
        return Ok(confirmationLink);
    }

    // password Rest
    [AllowAnonymous]
    [HttpPost("passwordReset")]
    public async Task<IActionResult> PasswordReset(string userId, string token, [FromBody] PasswordChangeRequest passwordRequest)
    {
        if (String.IsNullOrEmpty(userId)
         || String.IsNullOrEmpty(token)
         || String.IsNullOrEmpty(passwordRequest.newPassword))
            return BadRequest(new { message = "Invalid request. User ID and token are required." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest("Double check the values you've provided please.");

        var result = await _userManager.ResetPasswordAsync(user, token, passwordRequest.newPassword);
        return result.Succeeded
            ? Ok("Success!\nPlease login again.")
            : BadRequest("Something went wrong please try again later");
    }

    // Email confirmation request

    [AllowAnonymous]
    [HttpPost("requestEmailConfirmation")]
    public async Task<IActionResult> RequestEmailConfirmation([FromBody] ConfirmEmailRequest emailConfirmationRequest)
    {
        var user = await _userManager.FindByEmailAsync(emailConfirmationRequest.Email);
        if (user is null)
            return BadRequest();

        if (user.EmailConfirmed)
            return Ok("email is already confirmed!");

        var emailConfirmToken = Uri.EscapeDataString(await _userManager.GenerateEmailConfirmationTokenAsync(user));
        var confirmationLink = GenerateConfirmationLink(user, emailConfirmToken, nameof(ConfirmEmail));

        //TODO send the email not return it in the request!
        return Ok(confirmationLink);
    }

    [HttpGet]
    [AllowAnonymous]
    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(token))
            return BadRequest(new { message = "Invalid request. User ID and token are required." });

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        token = Uri.UnescapeDataString(token);
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
            return Ok(new EmailConfirmationResponse("Email confirmed successfully."));
        else
            return BadRequest(new EmailConfirmationResponse("Email confirmation failed."));
    }

    [Authorize]
    [HttpGet("currentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userEmail = User.Claims?.FirstOrDefault(x => x.Type == "sub")?.Value;
        var user = await _userManager.FindByNameAsync(userEmail!);

        if (user == null) return BadRequest();
        var userRoles = await _userManager.GetRolesAsync(user);

        return Ok(
            new CurrentUserResponse(
                user.Email!,
                user.UserName!,
                userRoles.ToArray()
            )
        );
    }

    [AllowAnonymous]
    [HttpPost("username")]
    public async Task<IActionResult> CheckUsername(UserNameCheckRequest nameCheckRequest)
    {
        // make a custom SQL cause u only need a bool yo, no need for the entire use object . . .
        var res = await _userManager.FindByNameAsync(nameCheckRequest.Username);

        return res is not null
            ? Ok(new UserNameCheckResponse(isAvailable: false, message: "Username is not available! ;=;"))
            : Ok(new UserNameCheckResponse(isAvailable: true, message: "Username is available! :D"));
    }

    [AllowAnonymous]
    [HttpPost("email")]
    public async Task<IActionResult> CheckEmail(EmailCheckRequest emailCheckRequest)
    {
        // make a custom SQL cause u only need a bool yo, no need for the entire use object . . .
        var res = await _userManager.FindByEmailAsync(emailCheckRequest.Email);

        return res is not null
            ? Ok(new EmailCheckResponse(isAvailable: false, message: "Email is not available! ;=;"))
            : Ok(new EmailCheckResponse(isAvailable: true, message: "Email is available! :D"));
    }

    private string GenerateConfirmationLink(User user, string emailConfirmToken, string controllerName)
    {
        // <Request.Scheme> :// <Request.Host> /api/auth/ConfirmEmail?userId=user.Id&token=emailConfirmationToken
        var confirmationLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/auth/{controllerName}?userId={user.Id}&token={emailConfirmToken}";
        return confirmationLink;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile(
        [FromQuery] int commentPage = 1,
        [FromQuery] int commanePageSize = 10,
        [FromQuery] int postsPage = 1,
        [FromQuery] int postsPageSize = 10)
    {
        var userName = _userAccessor.GetUsername();

        if (userName == null) return Unauthorized();

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);

        //todo: Get user's comment
        var userComments = await _commentRepo.GetAllCommentsForUser(user.Id, commentPage, commanePageSize);
        var userFavorites = await _postRepo.GetUsersFavoritePosts(user.Id, postsPage, postsPageSize);

        if (!userComments.IsSuccess || !userFavorites.IsSuccess)
            return Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later.");

        return Ok(
            new UserProfileResponse(
                username: user.UserName!,
                email: user.Email!,
                profilePicture: user.ProfilePicture,
                Roles: userRoles.ToArray(),
                comments: userComments.Value,
                favoritePosts: userFavorites.Value

                )
            );
    }
}