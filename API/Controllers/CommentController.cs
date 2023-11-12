// Ignore Spelling: repo Accessor API

using Application.Security;

using DataAccess.Dtos;
using DataAccess.Repos;
using DataAccess.Shared;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepo _repo;
        private readonly IUserAccessor _userAccessor;

        public CommentController(ICommentRepo repo, IUserAccessor userAccessor)
        {
            _repo = repo;
            _userAccessor = userAccessor;
        }


        [HttpGet("{postId}")]
        public async Task<IActionResult> GetAll(Guid postId)
        {
            var comments = await _repo.GetCommentsForPost(postId);

            return comments.Operation switch
            {
                OperationStatus.Success => Ok(comments.Value),
                OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
                _ => BadRequest()
            } ;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto newComment)
        { 
            var authorId = _userAccessor.GetUserId();

            if (authorId is null) return Unauthorized();

            var res = await _repo.UpsertComment(newComment,  authorId!);

            return res.Operation switch
            {
                OperationStatus.Created => CreatedAtAction(nameof(CreateComment), new { id = res.Value.Id }, res.Value),
                OperationStatus.Updated => Ok(res.Value),
                OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
                _ => BadRequest()
            };
        }


        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var authorId = _userAccessor.GetUserId();

            if (authorId is null) return Unauthorized();

            var res = await _repo.SoftDeleteComment(commentId, authorId!);

            return res.Operation switch
            {
                OperationStatus.Deleted => NoContent(),
                OperationStatus.NotFound=> Unauthorized(),
                OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
                _ => BadRequest()
            };
        }

    }
}
