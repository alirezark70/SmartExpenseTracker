using Mapster;
using SmartExpenseTracker.Core.ApplicationService.Contracts.Base;
using SmartExpenseTracker.Core.ApplicationService.Dtos.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.MappingConfig
{
    public class PostConfig : IMappingConfig
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PostDto, PostViewModel>()
                .Map(des => des.Descritpion, sour => sour.Body + " " + sour.Id)
            .IgnoreNullValues(true);
        }
    }
}
