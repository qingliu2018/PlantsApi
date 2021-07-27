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
    public class LessiController : ControllerBase
    {
        private static readonly string[] Users = new[]
        {
            "User1", "User2", "User3"
        };
        private readonly ILogger<LessiController> _logger;

        public LessiController(ILogger<LessiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]//.../Users
        public IList<string> Get()
        {

            var connectionString =
                "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";
            var tableClient = new TableClient(connectionString, "Users");
            var entities = tableClient.Query<TableEntity>();

            var UserNames = entities.Select(x => $"Lessi - {x["UserName"].ToString()}").ToList();

            return UserNames;
        }
    }
}
