using Microsoft.Extensions.Logging;
using SmartExpenseTracker.Core.ApplicationService.Contracts.ExternalServices.Posts;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Posts;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.ExternalServices.Posts
{
    public class PostInquiryService : IPostInquiryService
    {
        private readonly HttpClient _httpClient;
        private readonly IMappingService _mapper;
        private readonly ILogger<PostInquiryService> _logger;

        public PostInquiryService(HttpClient httpClient, IMappingService mapper, ILogger<PostInquiryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            Id = Guid.NewGuid();
            _mapper = mapper;
        }

        public Guid Id { get; init; }

        public async Task<IEnumerable<PostDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
               $"posts",
               cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Enumerable.Empty<PostDto>();


            // await Task.Delay(5000);
            response.EnsureSuccessStatusCode();

            var person = await response.Content.ReadFromJsonAsync<IEnumerable<PostDto>>(cancellationToken);
            return person ?? Enumerable.Empty<PostDto>();
        }

        public async Task<PostDto?> GetPostByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
               $"posts/{id}",
               cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;


            // await Task.Delay(5000);
            response.EnsureSuccessStatusCode();

            var person = await response.Content.ReadFromJsonAsync<PostDto>(cancellationToken);
            return person;
        }

        //public async Task<IEnumerable<PostDto>> SearchPostsAsync(PostSearchCriteria criteria, CancellationToken cancellationToken = default)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
