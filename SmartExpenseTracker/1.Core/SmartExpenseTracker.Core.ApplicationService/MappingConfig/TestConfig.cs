using Mapster;
using SmartExpenseTracker.Infra.Mapping.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Core.ApplicationService.MappingConfig
{
    public class TestConfig : IMappingConfig
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Test, TestDto>()
            .IgnoreNullValues(true);
        }
    }


    public class TestDto
    {
        public int Id { get; set; }
    }

    public class Test
    {
        public int Id { get; set; }
    }
}
