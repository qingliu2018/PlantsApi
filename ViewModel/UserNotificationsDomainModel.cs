using System;
using System.Text.Json.Serialization;

namespace PlantsApi.ViewModel
{
    public class UserNotificationsDomainModel
    {
        //these come from storage
        public string RowKey { get; set; }
        public string UserRowKey { get; set; }
        public string PlantRowKey { get; set; }
        public DateTime NotificationDate { get; set; }
        public bool Read { get; set; }
        public string Description { get; set; }
    }
}