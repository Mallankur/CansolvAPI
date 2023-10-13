using CansolveApplication.CansolveApplicationServises;
using CansolveApplication.Model;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CansolveApplication.Controllers.CansolveController
{
    [Route("api/[controller]")]
    [ApiController]
    public class CansolveController : ControllerBase
    {
        private readonly IcansolveServises _Servises; 
        public CansolveController(IcansolveServises servises)
        {
                _Servises = servises;
        }
        // GET: api/<CansolveController>
        [HttpGet]
        public async  Task<IEnumerable<CansolveEntity>>GetAsync(DateTime  eventTime)
        {
          return  await _Servises.GetByEvenTimeAsync(eventTime);
        }

        

        [HttpGet("{EventTimeAvgCalculations}")]
        public async Task<AggregationModelResult>GetAvgValue(DateTime EventTimeAvgCalculations)
        {
            return await _Servises.GetAvgValue(EventTimeAvgCalculations); 
        }

        
    }
}
