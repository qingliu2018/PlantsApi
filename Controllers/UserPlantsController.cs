using System;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantsApi.Model;

namespace PlantsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserPlantsController : ControllerBase
    {
        private readonly ILogger<UserPlantsController> _logger;
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";

        public UserPlantsController(ILogger<UserPlantsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]//.../Users
        public async Task<AsyncPageable<TableEntity>> Get()
        {
            var tableClient = new TableClient(ConnectionString, "UserPlants");
            var entities = tableClient.QueryAsync<TableEntity>();
            return entities;
        }

        [HttpPost]
        public async Task Post(UserPlant input)
        {
            var tableClient = new TableClient(ConnectionString, "UserPlants");

            var entity = new TableEntity("UserPlants", Guid.NewGuid().ToString())
            {
                { "UserRowKey", input.UserRowKey },
                { "PlantRowKey", input.PlantRowKey },
                { "OwnershipDate", input.OwnershipDate ?? DateTime.Now},
                { "LastRepotted", input.LastRepotted ?? DateTime.Now },
                { "LastWatered", input.LastWatered ?? DateTime.Now },
            };

            await tableClient.AddEntityAsync(entity);
        }
    }
}