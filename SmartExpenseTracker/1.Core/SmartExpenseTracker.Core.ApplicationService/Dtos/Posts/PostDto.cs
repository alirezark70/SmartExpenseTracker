using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Dtos.Posts
{
    public record PostDto(
 int UserId, int Id, string Title, string Body);
}
