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
        };

        var res = await _userManager.CreateAsync(user, userCreds.Password);

        if (!res.Succeeded)
        {
            foreach (var error in res.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return ValidationProblem();
        }

        await _userManager.AddToRoleAsync(user, "User");

        //TODO:  place holder for the actual response
        //TODO: can you navigate a user from hserer ? ?? ? ? ?
        return StatusCode(201);
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

    // [HttpPost]
    // public async Task<IActionResult> ResetPassword()
    // {
    //     return Ok();
    // }

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