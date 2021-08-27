using System;

namespace PlantsApi.Model
{
    public class UserPlantDomainModel
    {
        //these come from storage
        public string UserPlantRowKey { get; set; }
        public string PlantRowKey { get; set; }
        public string PlantName { get; set; }
        public string PlantPhotoUri { get; set; }
        public string PlantWikipediaUri { get; set; }
        public DateTime OwnershipDate { get; set; }
        public DateTime LastWatered { get; set; }
        public DateTime LastRepotted { get; set; }
        public int WateringPeriodInDays { get; set; }
        public int RepottingPeriodInDays { get; set; }

        //read only, calculated properties
        public int WateringDueInDays => (int)(LastWatered - DateTime.Now).TotalDays + WateringPeriodInDays;
        public bool WateringDue => WateringDueInDays <= 0;
        public int RepottingDueInDays => (int)(LastRepotted - DateTime.Now).TotalDays + RepottingPeriodInDays;
        public bool RepottingDue => RepottingDueInDays <= 0;
    }
}
