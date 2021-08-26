using System;

namespace PlantsApi.Model
{
    public class UserPlantDetails
    {
        public string UserPlantRowKey { get; set; }
        // public string UserRowKey { get; set; }
        // public string UserName { get; set; }
        public string PlantRowKey { get; set; }
        public string PlantName { get; set; }
        public string WateringPeriodInDays { get; set; }
        public string PlantPhotoUri { get; set; }
        public string PlantWikipediaUri { get; set; }
        public DateTime LastWatered { get; set; }
    }
}