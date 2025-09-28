using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.Common.Security
{
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute() { }

        public string? Roles { get; set; }
        public string? Permissions { get; set; }
        public string? Policy { get; set; }
    }
}
