using Application.Security;
using DataAccess.Dtos;
using DataAccess.Repos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostRepo _postRepo;
    private readonly IJwtGenerator _jwtGenerator;
    public PostController(IPostRepo postRepo, IJwtGenerator jwtGenerator)
    {
        _jwtGenerator = jwtGenerator;
        _postRepo = postRepo;
        
    }

    [HttpGet]
    public async Task<IActionResult> Get ()
    {
        var posts = await _postRepo.GetAll();

        switch (posts.Operation)
        {
            case OperationStatus.Success:
                return Ok( new PostResponse<List<PostReadDto>>(
                    posts.Operation.ToString(),
                      posts.Value,
                      _jwtGenerator.GenerateToken(new Domain.Entities.User{
                        Id = Guid.NewGuid(),
                        DisplayName = "Canaan",
                        Email= "canaan@example.com",
                        Password = "wut?!"
                      })
                      
                      ));
            case OperationStatus.Error:
                return StatusCode(500, "Something went wrong processing your request, please try again later.");
            default:
                return BadRequest();
        }
    }

    [HttpGet("slug")]
    public async Task<IActionResult> GetPost(string slug)
    {
        var post = await _postRepo.GetBySlug(slug);
        switch (post.Operation)
        {
            case OperationStatus.Success:
                return Ok(post.Value);
            case OperationStatus.Error:
                return StatusCode(500, "Something went wrong processing your request, please try again later.");
            default:
                return BadRequest();
        }

    }

    [HttpPut("id:Guid")]
    public async Task<IActionResult> UpSert(Guid id, [FromBody] PostUpsertRequest post)
    {
        var res = await _postRepo.UpsertPost(id,new PostUpsertDto{ Title = post.Title, Body = post.Body  } );
        switch (res.Operation)
        {
            case OperationStatus.Created:
                return CreatedAtAction(nameof(Create), new { id = res.Value.Id }, res.Value);
            case OperationStatus.Updated:
                return NoContent();
            case OperationStatus.Error:
                return StatusCode(500, "Something went wrong processing your request, please try again later.");
            default:
                return BadRequest();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostUpsertRequest post)
    {
        var res = await _postRepo.CreatePost( new PostUpsertDto { Title = post.Title, Body = post.Body });
        switch (res.Operation)
        {
            case OperationStatus.Created:
                return CreatedAtAction( nameof(Create),new {id = res.Value.Id}, res.Value);
            case OperationStatus.Error:
                return StatusCode(500, "Something went wrong processing your request, please try again later.");
            default:
                return BadRequest();
        }
    }

    [HttpDelete("id:Guid")]
    public async Task<IActionResult> Delete (Guid id)
    {
        var res = await _postRepo.Delete(id);
        switch (res.Operation)
        {
            case OperationStatus.Deleted:
                return NoContent();
            case OperationStatus.Error:
                return StatusCode(500, "Something went wrong processing your request, please try again later.");
            default:
                return BadRequest();
        }
    }

}

public record PostResponse<T> (
    string Status,
    T Value,
    string Token
);

public record PostUpsertRequest(
    string Title,
    string Body
);