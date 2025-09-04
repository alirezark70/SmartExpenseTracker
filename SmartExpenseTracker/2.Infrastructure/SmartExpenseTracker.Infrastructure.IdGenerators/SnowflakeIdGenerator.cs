using IdGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infrastructure.IdGenerators
{
    public sealed class SnowflakeIdGenerator(IdGen.IdGenerator generator) : SmartExpenseTracker.Core.ApplicationService.Contracts.IIdGenerator<long>
    {
        private readonly IdGen.IdGenerator _generator = generator;
        public long GetId()
        {
           return _generator.CreateId();
        }
    }
}
