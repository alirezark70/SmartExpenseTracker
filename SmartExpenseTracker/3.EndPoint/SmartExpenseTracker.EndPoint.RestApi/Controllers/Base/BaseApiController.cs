using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities;
using SmartExpenseTracker.Core.Domain.Enums.Response;
using SmartExpenseTracker.EndPoint.RestApi.Filters;
using SmartExpenseTracker.Infra.Mapping.Contracts;

namespace SmartExpenseTracker.EndPoint.RestApi.Controllers.Base
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiResponseActionFilter]
    public class BaseApiController : ControllerBase
    {
        protected readonly ILogger<PostController> _logger;
        protected readonly IMappingService _mappingService;
        protected readonly IMediator _mediator;
        public BaseApiController(ILogger<PostController> logger, IMappingService mappingService, IMediator mediator)
        {
            _logger = logger;
            _mappingService = mappingService;
            _mediator = mediator;
        }
        protected IActionResult OkResponse<T>(T data, string message = "عملیات با موفقیت انجام شد")
        {
            return Ok(ApiResponse<T>.Success(data, message));
        }

        protected IActionResult CreatedResponse<T>(T data, string message = "با موفقیت ایجاد شد")
        {
            return StatusCode(201, ApiResponse<T>.Created(data, message));
        }

        protected IActionResult NoContentResponse()
        {
            return Ok(ApiResponse.NoContent());
        }

        protected IActionResult BadRequestResponse(string message)
        {
            return BadRequest(ApiResponse.Failure(message, ResponseStatus.BadRequest));
        }

        protected IActionResult NotFoundResponse(string message)
        {
            return NotFound(ApiResponse.Failure(message, ResponseStatus.NotFound));
        }

        protected IActionResult PagedResponse<T>(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            return Ok(new PagedResponse<T>(data, pageNumber, pageSize, totalRecords));
        }
    }
}
