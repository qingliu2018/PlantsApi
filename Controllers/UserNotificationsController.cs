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
    public class UserNotificationsController : ControllerBase
    {
        private readonly ILogger<UserNotificationsController> _logger;
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=plantappstorage;AccountKey=H+ox9U/nzArLKVVnvcfIWV1K02xNnXFipfKXUfttZaoB0FB6DYRj5SKf4F8487xbUtmPpxzJIh9lMwiKw+jAfA==;EndpointSuffix=core.windows.net";

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
        /// Adds a notification reminding user about an action on a plant
        /// If there is an existing User and Plant row key combo, the notification
        /// will get updated with the latest data and reset to read = false
        /// </summary>
        /// <param name="input"></param>
        /// <returns>notification details</returns>
        [HttpPost]
        public async Task<ActionResult> Post(UserNotificationsDomainModel input)
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
                {"UserPlantKey", input.UserRowKey},
                {"NotificationDate", DateTime.Now},
                {"Read", false}, //notification always starts off as unread
                {"Description", input.Description},
            };
            await tableClient.AddEntityAsync(entity);
            return new JsonResultWithHttpStatus(entity, HttpStatusCode.Created);
        }

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