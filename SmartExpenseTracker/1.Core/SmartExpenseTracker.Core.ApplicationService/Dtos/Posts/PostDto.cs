using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Dtos.Posts
{
    public record PostDto(
 int UserId, int Id, string Title, string Body);

    public class PostViewModel
    {
        public int UserId
        {
            get; set;
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public string Body { get; set; }

        public string Descritpion { get; set; }

    }
}
