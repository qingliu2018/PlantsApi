using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantsApi.ViewModel;

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

        /// <summary>
        /// Adds a user plant.
        /// NOTE: The system doesn't currently support owning more than two types of plant
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post(UserPlantDomainModel input)
        {
            //step 1: check if user already owns a plant of this type
            var tableClient = new TableClient(ConnectionString, "UserPlants");
            
            var filters = new List<string>
            {
                $"UserRowKey eq '{input.UserRowKey}'",
                $"PlantRowKey eq '{input.PlantRowKey}'"
            };
            var filter = string.Join(" and ", filters);
            var entity = tableClient.Query<TableEntity>(filter).FirstOrDefault();

            if (entity != null) return new JsonResultWithHttpStatus($"CONFLICT: User already owns that plant!", HttpStatusCode.Conflict); //if existing record found tell the user they already own this plant

            if (input.OwnershipDate == DateTime.MinValue) { input.OwnershipDate = DateTime.Now; } //if dates not specified, use today's
            if (input.LastRepotted == DateTime.MinValue) { input.LastRepotted = DateTime.Now; } //if dates not specified, use today's
            if (input.LastWatered == DateTime.MinValue) { input.LastWatered = DateTime.Now; } //if dates not specified, use today's

            entity = new TableEntity("UserPlants", Guid.NewGuid().ToString())
            {
                { "UserRowKey", input.UserRowKey },
                { "PlantRowKey", input.PlantRowKey },
                { "OwnershipDate", input.OwnershipDate},
                { "LastRepotted", input.LastRepotted },
                { "LastWatered", input.LastWatered },
            };
            await tableClient.AddEntityAsync(entity);
            return new JsonResultWithHttpStatus(entity, HttpStatusCode.Created);
        }
    }
}