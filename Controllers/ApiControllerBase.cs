using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using PlantsApi.Model;
using PlantsApi.ViewModel;

namespace PlantsApi.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        protected const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";

        /// <summary>
        /// Returns actual user plants
        /// </summary>
        /// <param name="userRowKey">Optional parameter</param>
        /// <returns></returns>
        protected List<UserPlantDomainModel> GetUserPlants(string userRowKey = null)
        {
            //step 1: get user plants associations
            var userPlantsTableClient = new TableClient(ConnectionString, "UserPlants");
            Pageable<TableEntity> userPlants;
            if (!string.IsNullOrEmpty(userRowKey))
            {
                var getUserPlantsFilter = string.Join(" and ", new List<string> {$"UserRowKey eq '{userRowKey}'"});
                userPlants = userPlantsTableClient.Query<TableEntity>(getUserPlantsFilter);
            }
            else
            {
                userPlants = userPlantsTableClient.Query<TableEntity>(); //get all plants (for generating notifications)
            }

            var plantsOwnedIds = userPlants
                .Select(x => new UserPlant
                {
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

        private static TableEntity GetPlantDetailsByPlantRowKey(UserPlant poi)
        {
            var plantsTableClient = new TableClient(ConnectionString, "Plants");
            var getPlantsFilter = string.Join(" and ", $"RowKey eq '{poi.PlantRowKey}'");
            var plant = plantsTableClient.Query<TableEntity>(getPlantsFilter).FirstOrDefault();
            return plant;
        }

        protected static async Task<ActionResult> RaiseNotification(UserNotificationsDomainModel input)
        {
            var tableClient = new TableClient(ConnectionString, "Notifications");

            var filters = new List<string>
            {
                $"UserRowKey eq '{input.UserRowKey}'",
                $"PlantRowKey eq '{input.PlantRowKey}'"
            };
            var filter = string.Join(" and ", filters);

            var entity = tableClient.Query<TableEntity>(filter).FirstOrDefault();

            if (entity != null) //if notification for that user plant is there, update it
            {
                entity["NotificationDate"] = DateTime.Now;
                entity["Read"] = false;
                entity["Description"] = input.Description;
                await tableClient.UpdateEntityAsync(entity, ETag.All);
                return new JsonResultWithHttpStatus(entity, HttpStatusCode.OK);
            }

            entity = new TableEntity("Notifications", Guid.NewGuid().ToString())
            {
                {"UserRowKey", input.UserRowKey},
                {"PlantRowKey", input.PlantRowKey},
                {"NotificationDate", DateTime.Now},
                {"Read", false}, //notification always starts off as unread
                {"Description", input.Description},
            };
            await tableClient.AddEntityAsync(entity);
            return new JsonResultWithHttpStatus(entity, HttpStatusCode.Created);
        }

        protected static string GenerateAttentionMessage(UserPlantDomainModel plant)
        {
            var attention = new StringBuilder();
            if (plant.WateringDue)
            {
                attention.Append("watering");
                if (plant.RepottingDue)
                {
                    attention.Append(" and repotting");
                }
            }
            else if (plant.RepottingDue)
            {
                attention.Append("repotting");
            }

            return attention.ToString();
        }
    }
}