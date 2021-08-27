using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet]
        public async Task<AsyncPageable<TableEntity>> Get()
        {
            var tableClient = new TableClient(ConnectionString, "UserPlants");
            var entities = tableClient.QueryAsync<TableEntity>();
            return entities;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<AsyncPageable<TableEntity>> Get(string id)
        {
            var tableClient = new TableClient(ConnectionString, "UserPlants");
            var filters = new List<string> { $"RowKey eq '{id}'" };
            var filter = string.Join(" and ", filters);
            var entities = tableClient.QueryAsync<TableEntity>(filter);
            return entities;
        }

        [HttpPut]
        [Route("{id}/Watered")]
        public async Task<TableEntity> PlantWatered(string id)
        {
            var tableClient = new TableClient(ConnectionString, "UserPlants");
            TableEntity entity = await tableClient.GetEntityAsync<TableEntity>("UserPlants", id);
            entity["LastWatered"] = DateTime.Now;
            await tableClient.UpdateEntityAsync(entity, ETag.All);
            return entity;
        }

        [HttpPut]
        [Route("{id}/Repotted")]
        public async Task<TableEntity> PlantRepotted(string id)
        {
            var tableClient = new TableClient(ConnectionString, "UserPlants");
            TableEntity entity = await tableClient.GetEntityAsync<TableEntity>("UserPlants", id);
            entity["LastRepotted"] = DateTime.Now;
            await tableClient.UpdateEntityAsync(entity, ETag.All);
            return entity;
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