using DataAccess.Repos;
using DataAccess.Shared;

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ITagRepo _repo;

        public TagController(ITagRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _repo.GetAllTags();

            return tags.Operation switch
            {
                OperationStatus.Success => Ok(tags.Value),
                OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
                _ => BadRequest()

            };
        }

        [HttpGet("{Name}")]
        public async Task<IActionResult> GetTagByName(string name)
        {
            var tag = await _repo.GetTagById(name);
            return tag.Operation switch
            {
                OperationStatus.Success => Ok(tag.Value),
                OperationStatus.Error => Problem(statusCode: 500, detail: "Something went wrong processing your request, please try again later."),
                _ => BadRequest()

            };

        }


    }
}
