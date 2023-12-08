// Ignore Spelling: Sert jwt Repo

using API.Contracts;

using Application.Security;

using DataAccess.Dtos;
using DataAccess.Repos;
using DataAccess.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostRepo _postRepo;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUserAccessor _userAccessor;

    public PostController(IPostRepo postRepo, IJwtGenerator jwtGenerator, IUserAccessor userAccessor)
    {
        _jwtGenerator = jwtGenerator;
        _userAccessor = userAccessor;
        _postRepo = postRepo;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllPosts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null,
    [FromQuery] string sortBy = "publish_date",
    [FromQuery] bool isSortAscending = true,
    [FromQuery] string? filterValue = null,
    [FromQuery] string? filterType = null)
    {
        var posts = await _postRepo.GetAll(page, pageSize, searchTerm, sortBy, isSortAscending, filterValue, filterType);

        return posts.Operation switch
        {
            OperationStatus.Success => Ok(posts.Value),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpGet("archive/all")]
    public async Task<IActionResult> GetPostsForArchive()
    {
        var posts = await _postRepo.GetArchivePosts();

        return posts.Operation switch
        {
            OperationStatus.Success => Ok(posts.Value),
            OperationStatus.NotFound => NotFound(),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpGet("no-pagination/all")]
    public async Task<IActionResult> GetAllPosts()
    {
        var posts = await _postRepo.GetAllPostsGroupedByCategory();

        return posts.Operation switch
        {
            OperationStatus.Success => Ok(posts.Value),
            OperationStatus.NotFound => NotFound(),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPost(string slug)
    {
        var post = await _postRepo.GetBySlug(slug);

        return post.Operation switch
        {
            OperationStatus.Success => Ok(post.Value),
            OperationStatus.NotFound => NotFound(),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost]
    public async Task<IActionResult> UpSert([FromBody] PostUpsertRequest post)
    {
        var authorId = _userAccessor.GetUserId();

        if (authorId is null) return Unauthorized();

        var tags = new List<TagUpsertDto>();
        var categories = new List<CategoryUpsertDto>();

        foreach (var category in post.Categories.Trim().Split(','))
            categories.Add(new CategoryUpsertDto { Name = category});

        foreach (var tag in post.Tags.Trim().Split(','))
            tags.Add(new TagUpsertDto { Name = tag });

        var res = await _postRepo.UpsertPost(
            authorId,
            string.IsNullOrEmpty(post.Id.Trim()) ? Guid.Empty : Guid.Parse(post.Id),
            new PostUpsertDto
            {
                Title = post.Title,
                Content = JsonConvert.DeserializeObject( post.Body).ToString(), // here untill i decide on what kinda editor i wanna use
                Tags = tags,
                Categories = categories,
                IsPublished = post.IsPublised
            });
        return res.Operation switch
        {
            OperationStatus.Created => CreatedAtAction(nameof(GetPost), new { slug = res.Value.Slug }, res.Value),
            OperationStatus.Updated => NoContent(),
            OperationStatus.NotFound => Unauthorized(),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [Authorize]
    [HttpPost("react")]
    public async Task<IActionResult> ReactToPost([FromBody] PostReactionRequest postReaction)
    {
        var userId = _userAccessor.GetUserId();
        if (userId is null) return Unauthorized();

        var res = await _postRepo.UpSertPostReaction(userId, Guid.Parse(postReaction.PostId), postReaction.ReactionType);

        return res.Operation switch
        {
            OperationStatus.Created => Ok(),
            OperationStatus.Updated => NoContent(),
            OperationStatus.Deleted => NoContent(),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var authorId = _userAccessor.GetUserId();

        if (authorId is null) return Unauthorized();

        var res = await _postRepo.SoftDelete(id);
        return res.Operation switch
        {
            OperationStatus.Deleted => NoContent(),
            OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
            _ => BadRequest()
        };
    }
}