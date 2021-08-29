using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using PlantsApi.Model;
using PlantsApi.ViewModel;

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

        [HttpGet]
        public Pageable<TableEntity> Get()
        {
            var tableClient = new TableClient(ConnectionString, "Users");
            var entities = tableClient.Query<TableEntity>();
            return entities;
        }

        [HttpGet]
        [Route("{id}")]
        public TableEntity Get(string id)
        {
            var tableClient = new TableClient(ConnectionString, "Users");
            var filters = new List<string> {$"RowKey eq '{id}'"};
            var filter = string.Join(" and ", filters);

            var entities = tableClient.Query<TableEntity>(filter);
            return entities.FirstOrDefault();
        }

        [HttpGet]
        [Route("{id}/Plants")]
        public List<UserPlantDomainModel> Plants(string id)
        {
            //step 1: get user plants associations
            var userPlantsTableClient = new TableClient(ConnectionString, "UserPlants");
            var getUserPlantsFilter = string.Join(" and ", new List<string> { $"UserRowKey eq '{id}'" });
            var userPlants = userPlantsTableClient.Query<TableEntity>(getUserPlantsFilter);
            var plantsOwnedIds = userPlants
                .Select(x => new UserPlant{
                 RowKey = x.GetString("RowKey"),
                 PlantRowKey = x.GetString("PlantRowKey"),
                 UserRowKey = x.GetString("UserRowKey"),
                 OwnershipDate = x.GetDateTimeOffset("OwnershipDate").GetValueOrDefault().Date,
                 LastRepotted = x.GetDateTimeOffset("LastRepotted").GetValueOrDefault().Date,
                 LastWatered = x.GetDateTimeOffset("LastWatered").GetValueOrDefault().Date,
             }).ToList();

            //step 2: hydrate with plant info
            if (plantsOwnedIds.Count <= 0) return null;

            var userPlantsDetails = new List<UserPlantDomainModel>();
            foreach (var poi in plantsOwnedIds)
            {
                var userPlantDetails = new UserPlantDomainModel
                {
                    UserRowKey = poi.UserRowKey,
                    PlantRowKey = poi.PlantRowKey,
                    UserPlantRowKey = poi.RowKey,
                    OwnershipDate = poi.OwnershipDate ?? DateTime.MinValue,
                    LastWatered = poi.LastWatered ?? DateTime.MinValue,
                    LastRepotted = poi.LastRepotted ?? DateTime.MinValue,
                };
                var plantDetails = GetPlantDetailsByPlantRowKey(poi);
                if (plantDetails != null)
                {
                    userPlantDetails.PlantName = plantDetails.GetString("Name");
                    userPlantDetails.WateringPeriodInDays = plantDetails.GetInt32("WateringPeriodInDays") ?? 15;
                    userPlantDetails.RepottingPeriodInDays = plantDetails.GetInt32("RepottingPeriodInDays") ?? 15;
                    userPlantDetails.PlantPhotoUri = plantDetails.GetString("PlantPhotoUri");
                    userPlantDetails.PlantWikipediaUri = plantDetails.GetString("PlantWikipediaUri");
                }

                userPlantsDetails.Add(userPlantDetails);
            }
            return userPlantsDetails;
        }

        [HttpGet]
        [Route("{id}/Notifications")]
        public List<UserNotificationsDomainModel> Notifications(string id)
        {
            //step 1: get user notifications
            var tableClient = new TableClient(ConnectionString, "Notifications");
            var userNotificationsFilter = string.Join(" and ", new List<string>
            {
                $"UserRowKey eq '{id}'",
                $"Read eq false" //we only get new notifications
            });
            var userNotifications = tableClient.Query<TableEntity>(userNotificationsFilter);

            var notificationsDomainModels = userNotifications
                .Select(x => new UserNotificationsDomainModel()
                {
                    RowKey = x.GetString("RowKey"),
                    Description = x.GetString("Description"),
                    NotificationDate = x.GetDateTimeOffset("NotificationDate").GetValueOrDefault().Date,
                    Read = x.GetBoolean("Read").GetValueOrDefault(),
                }).ToList();

          
            return notificationsDomainModels;
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
