using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartExpenseTracker.Core.ApplicationService.Contracts.ExternalServices.Posts;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Posts;
using SmartExpenseTracker.EndPoint.RestApi.Controllers.Base;
using SmartExpenseTracker.Infra.Mapping.Contracts;

namespace SmartExpenseTracker.EndPoint.RestApi.Controllers
{
   
    public class PostController : BaseApiController
    {
        private readonly ILogger<PostController> _logger;
        private readonly IMappingService _mappingService;
        private readonly IPostInquiryService _postInquiryService;

        public PostController(ILogger<PostController> logger, IMappingService mappingService, IPostInquiryService postInquiryService)
        {
            _logger = logger;
            _mappingService = mappingService;
            _postInquiryService = postInquiryService;
        }

        [HttpGet("GetPost/{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var result = await _postInquiryService.GetPostByIdAsync(id);
            return OkResponse<PostDto>(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postInquiryService.GetAllAsync();

            var dto = _mappingService.Map<IEnumerable<PostViewModel>>(posts);

            return PagedResponse<PostViewModel>(dto, 1, 1, dto.Count());
        }

        [HttpGet("GetAllTest")]
        public IEnumerable<PostDto> GetAllTest()
        {
            var posts = _postInquiryService.GetAllAsync();

            return posts.Result;
        }
    }
}
