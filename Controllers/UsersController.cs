using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using PlantsApi.Model;

namespace PlantsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]//.../Users
        public Pageable<TableEntity> Get()
        {
            var tableClient = new TableClient(ConnectionString, "Users");
            var entities = tableClient.Query<TableEntity>();
            return entities;
        }

        [HttpGet]//.../Users
        [Route("{id}")]
        public TableEntity Get(int id)
        {
            var tableClient = new TableClient(ConnectionString, "Users");
            var filters = new List<string>();
            filters.Add($"RowKey eq '{id}'");
            var filter = string.Join(" and ", filters);
            var entities = tableClient.Query<TableEntity>(filter);
            return entities.FirstOrDefault();
        }

        [HttpGet]
        [Route("{id}/Plants")]
        public List<UserPlantDetails> Plants(int id)
        {
            //step 1: get user plants associations
            var userPlantsTableClient = new TableClient(ConnectionString, "UserPlants");
            var getUserPlantsFilter = string.Join(" and ", new List<string> { $"UserRowKey eq '{id}'" });
            var userPlants = userPlantsTableClient.Query<TableEntity>(getUserPlantsFilter);
            var plantsOwnedIds = userPlants.Select(x => new UserPlant{
                 RowKey = x["RowKey"].ToString(),
                 PlantRowKey = x["PlantRowKey"].ToString(),
                 UserRowKey = x["UserRowKey"].ToString()
             }).ToList();

            //step 2: hydrate with plant info
            if (plantsOwnedIds.Count <= 0) return null;

            var userPlantsDetails = new List<UserPlantDetails>();
            foreach (var poi in plantsOwnedIds)
            {
                var userPlantDetails = new UserPlantDetails
                {
                    
                    PlantRowKey = poi.PlantRowKey,
                    UserPlantRowKey = poi.RowKey,
                    LastWatered = poi.LastWatered
                };

                var plantDetails = GetPlantDetailsByPlantRowKey(poi);
                if (plantDetails != null)
                {
                    userPlantDetails.PlantName = plantDetails["Name"].ToString();
                    // userPlantDetails.WateringPeriodInDays = plantDetails["WateringPeriodInDays"].ToString();
                    // userPlantDetails.PlantPhotoUri = plantDetails["PlantPhotoUri"].ToString();
                    // userPlantDetails.PlantWikipediaUri = plantDetails["PlantWikipediaUri"].ToString();
                }

                userPlantsDetails.Add(userPlantDetails);
            }
            return userPlantsDetails;
        }

        private static TableEntity GetPlantDetailsByPlantRowKey(UserPlant poi)
        {
            var plantsTableClient = new TableClient(ConnectionString, "Plants");
            var getPlantsFilter = string.Join(" and ", $"RowKey eq '{poi.PlantRowKey}'");
            var plant = plantsTableClient.Query<TableEntity>(getPlantsFilter).FirstOrDefault();
            return plant;
        }
    }
}
