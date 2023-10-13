using CansolveApplication.Model;

namespace CansolveApplication.CansolveApplicationServises
{
    public interface IcansolveServises
    {
        Task<List<CansolveEntity>> GetByEvenTimeAsync( DateTime EventTime);
        Task<CansolveEntity>GetByIdAsync(String Tagname);
        Task<AggregationModelResult> GetAvgValue(DateTime EVENTtIME);

    }
}
