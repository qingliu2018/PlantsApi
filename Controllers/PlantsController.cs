using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Data.Tables;

namespace PlantsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlantsController : ControllerBase
    {
        private readonly ILogger<PlantsController> _logger;
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";

        public PlantsController(ILogger<PlantsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]//.../Plants
        public Pageable<TableEntity> Get()
        {
            var tableClient = new TableClient(ConnectionString, "Plants");
            var entities = tableClient.Query<TableEntity>();
            // var plantNames = entities.Select(x => x["Name"].ToString()).ToList();
            return entities;
        }
    }
}
