namespace SmartExpenseTracker.Infra.IdGenerators
{
    public sealed class SnowflakeGuidIdGenerator() : Core.ApplicationService.Contracts.IGuidIdGenerator
    {
        public Guid GetId()
        {
            return Guid.NewGuid();
        }
    }
}
