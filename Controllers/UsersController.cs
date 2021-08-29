using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using PlantsApi.ViewModel;

namespace PlantsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ApiControllerBase
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
            return GetUserPlants(id);
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

    }
}
