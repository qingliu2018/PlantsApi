using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace PlantsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlantsController : ControllerBase
    {
        private static readonly string[] Plants = new[]
        {
            "Monstera", "Banana", "Tits"
        };
        private readonly ILogger<PlantsController> _logger;

        public PlantsController(ILogger<PlantsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]//.../Plants
        public IList<string> Get()
        {

            var connectionString =
                "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";
            var tableClient = new TableClient(connectionString, "Plants");
            var entities = tableClient.Query<TableEntity>();

            var plantNames = entities.Select(x => x["Name"].ToString()).ToList();

            return plantNames;
        }
    }
}
