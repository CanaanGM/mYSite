using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using API.Contracts;

using Application.Security;

using DataAccess.Entities;

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

    public AccountController(UserManager<User> userManager, IJwtGenerator jwtGenerator)
    {
        _jwtGenerator = jwtGenerator;
        _userManager = userManager;
    }

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

        // TODO: email body generation
        string confirmationLink = await GenerateEmailConfirmationLink(user);

        return CreatedAtAction(nameof(Register), new RegisterResponse(confirmationLink));
    }

    private async Task<string> GenerateEmailConfirmationLink(User user)
    {
        var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        emailConfirmToken = Uri.EscapeDataString(emailConfirmToken);

        // <Request.Scheme> :// <Request.Host> /api/auth/ConfirmEmail?userId=user.Id&token=emailConfirmationToken
        var confirmationLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/auth/ConfirmEmail?userId={user.Id}&token={emailConfirmToken}";
        await _userManager.AddToRoleAsync(user, "User");
        return confirmationLink;
    }

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
                user.Email!,
                user.UserName!,
                token,
                userRoles.ToArray()
            ));
    }

    // todo
    // password confirmation request
    // password confirmation

    // Email confirmation request

    [AllowAnonymous]
    [HttpPost("requestEmailConfirmation")]
    public async Task<IActionResult> RequestEmailConfirmation([FromBody] ConfirmEmailRequest emailConfirmationRequest)
    {
        var user = await _userManager.FindByEmailAsync(emailConfirmationRequest.Email);
        if (user is null )
            return BadRequest();

        if (user.EmailConfirmed) return Ok("email is already confirmed!");

        var confirmationLink = await GenerateEmailConfirmationLink(user);
        //TODO send the email not return it in the request!

        return Ok(confirmationLink);
    }

    [HttpGet]
    [AllowAnonymous]
    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Invalid request. User ID and token are required." });
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        token = Uri.UnescapeDataString(token);
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            return Ok(new EmailConfirmationResponse("Email confirmed successfully." ));
        }
        else
        {
            return BadRequest(new EmailConfirmationResponse("Email confirmation failed." ));
        }
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
}