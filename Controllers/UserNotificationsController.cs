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
    public class UserNotificationsController : ApiControllerBase
    {
        private readonly ILogger<UserNotificationsController> _logger;

        public UserNotificationsController(ILogger<UserNotificationsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all notifications in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Pageable<TableEntity> Get()
        {
            var tableClient = new TableClient(ConnectionString, "Notifications");
            var entities = tableClient.Query<TableEntity>();
            return entities;
        }
        
        /// <summary>
        /// Get Notification
        /// </summary>
        /// <param name="id">notification row key</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public TableEntity Get(string id)
        {
            var tableClient = new TableClient(ConnectionString, "Notifications");
            var filters = new List<string> {$"RowKey eq '{id}'"};
            var filter = string.Join(" and ", filters);
        
            var entities = tableClient.Query<TableEntity>(filter);
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Goes through all User Plants and if any need watering or repotting it will raise a notification
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Generate")]
        public async Task<ActionResult> Generate()
        {
            var allUserPlants = GetUserPlants();
            var i = 0;
            foreach (var plant in allUserPlants.Where(plant => plant.WateringDue || plant.RepottingDue))
            {
                i++;
                var attention = GenerateAttentionMessage(plant);
                
                await RaiseNotification(new UserNotificationsDomainModel
                {
                    PlantRowKey = plant.PlantRowKey,
                    UserRowKey = plant.UserRowKey,
                    NotificationDate = DateTime.Now,
                    Read = false,
                    Description = $"Your {plant.PlantName} needs {attention}!"
                });
            }
            return new JsonResultWithHttpStatus($"Raised a total of {i} notifications", HttpStatusCode.OK);
        }

        //
        // /// <summary>
        // /// Adds a notification reminding user about an action on a plant
        // /// If there is an existing User and Plant row key combo, the notification
        // /// will get updated with the latest data and reset to read = false
        // /// </summary>
        // /// <param name="input"></param>
        // /// <returns>notification details</returns>
        // [HttpPost]
        // public async Task<ActionResult> Post(UserNotificationsDomainModel input)
        // {
        //     return await RaiseNotification(input);
        // }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        /// <param name="id">This is the notification row key</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/MarkAsRead")]
        public async Task<TableEntity> MarkAsRead(string id)
        {
            var tableClient = new TableClient(ConnectionString, "Notifications");
            TableEntity entity = await tableClient.GetEntityAsync<TableEntity>("Notifications", id);
            entity["Read"] = true;
            await tableClient.UpdateEntityAsync(entity, ETag.All);
            return entity;
        }

        /// <summary>
        /// Undo read notification
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Notification row key</returns>
        [HttpPut]
        [Route("{id}/MarkAsUnread")]
        public async Task<TableEntity> MarkAsUnread(string id)
        {
            var tableClient = new TableClient(ConnectionString, "Notifications");
            TableEntity entity = await tableClient.GetEntityAsync<TableEntity>("Notifications", id);
            entity["Read"] = false;
            await tableClient.UpdateEntityAsync(entity, ETag.All);
            return entity;
        }
    }
}