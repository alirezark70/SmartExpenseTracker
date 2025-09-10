using SmartExpenseTracker.Core.Domain.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.Domain.DomainModels.Response.Entities
{
    public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }

        public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            HasNext = PageNumber < TotalPages;
            HasPrevious = PageNumber > 1;
            IsSuccess = true;
            StatusCode = ResponseStatus.Success;
            Message = "اطلاعات با موفقیت بارگزاری شد";

            Meta = new Dictionary<string, object>
            {
                ["pageNumber"] = PageNumber,
                ["pageSize"] = PageSize,
                ["totalPages"] = TotalPages,
                ["totalRecords"] = TotalRecords,
                ["hasNext"] = HasNext,
                ["hasPrevious"] = HasPrevious
            };
        }
    }
}
