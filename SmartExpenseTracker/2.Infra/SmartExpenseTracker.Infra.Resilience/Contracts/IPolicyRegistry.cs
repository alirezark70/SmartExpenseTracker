using Polly;
using SmartExpenseTracker.Infra.Resilience.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Resilience.Contracts
{
    public interface IPolicyRegistry
    {
        ResiliencePipeline<T> GetPipeline<T>(PolicyType policyType);
        ResiliencePipeline GetPipeline(PolicyType policyType);
        void RegisterPipeline<T>(PolicyType policyType, ResiliencePipeline<T> pipeline);
        void RegisterPipeline(PolicyType policyType, ResiliencePipeline pipeline);
    }
}
