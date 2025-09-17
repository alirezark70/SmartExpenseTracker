using SmartExpenseTracker.Core.ApplicationService.Dtos.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Contracts.ExternalServices.Posts
{
    public interface IPostInquiryService
    {
        public Guid Id { get; init; }
        Task<PostDto?> GetPostByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PostDto>> GetAllAsync(CancellationToken cancellationToken = default);


        //Task<IEnumerable<PostDto>> SearchPostsAsync(PostSearchCriteria criteria, CancellationToken cancellationToken = default);
    }
}
