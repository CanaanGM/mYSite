using Application.Security;
using DataAccess.Dtos;
using DataAccess.Repos;
using Domain.Shared;
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

    [HttpGet("pageSize,page,searchTerm,sortBy,isAscSort ")]
    public async Task<IActionResult> Get (
        int pageSize = 5,
        int page = 1,
        string? searchTerm = null,
        string sortBy = "Id",
        bool isAscSort = true
    )
    {
        var posts = await _postRepo.GetAll(
            pageSize: pageSize,
            page: page,
            searchTerm:searchTerm,
            sortBy: sortBy,
            isSortAscending: isAscSort
            
            );

        return posts.Operation switch
        {
            OperationStatus.Success => Ok(new PostResponse<PagedList<PostReadDto>>(
                    posts.Operation.ToString(),
                      posts.Value,
                      _jwtGenerator.GenerateToken(new Domain.Entities.User
                      {
                          Id = Guid.NewGuid(),
                          DisplayName = "Canaan",
                          Email = "canaan@example.com",
                          Password = "wut?!"
                      })

                      )),
            OperationStatus.Error => StatusCode(500, "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpGet("slug")]
    public async Task<IActionResult> GetPost(string slug)
    {
        var post = await _postRepo.GetBySlug(slug);

       return  post.Operation switch 
        {
            OperationStatus.Success => Ok(post.Value),
            OperationStatus.Error => StatusCode(500, "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpPut("id:Guid")]
    public async Task<IActionResult> UpSert(Guid id, [FromBody] PostUpsertRequest post)
    {
        var res = await _postRepo.UpsertPost(id,new PostUpsertDto{ Title = post.Title, Body = post.Body  } );
        return res.Operation switch
        {
            OperationStatus.Created => CreatedAtAction(nameof(Create), new { id = res.Value.Id }, res.Value),
            OperationStatus.Updated => NoContent(),
            OperationStatus.Error => StatusCode(500, "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostUpsertRequest post)
    {
        var res = await _postRepo.CreatePost( new PostUpsertDto { Title = post.Title, Body = post.Body });
        return res.Operation switch
        {
            OperationStatus.Created => CreatedAtAction(nameof(Create), new { id = res.Value.Id }, res.Value),
            OperationStatus.Error => StatusCode(500, "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpDelete("id:Guid")]
    public async Task<IActionResult> Delete (Guid id)
    {
        var res = await _postRepo.Delete(id);
        return res.Operation switch
        {
            OperationStatus.Deleted => NoContent(),
            OperationStatus.Error => StatusCode(500, "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
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