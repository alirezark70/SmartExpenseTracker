using IdGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.IdGenerators
{
    public sealed class SnowflakeIdGenerator(IdGenerator generator) : Core.ApplicationService.Contracts.IIdGenerator<long>
    {
        private readonly IdGenerator _generator = generator;
        public long GetId()
        {
           return _generator.CreateId();
        }
    }
}
