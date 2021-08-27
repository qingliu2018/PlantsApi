using System;

namespace PlantsApi.Model
{
    public class UserPlant : BaseModel
    {
        public string RowKey { get; set; }
        public string UserRowKey { get; set; }
        public string PlantRowKey { get; set; }
        public DateTime? OwnershipDate { get; set; }
        public DateTime? LastWatered { get; set; }
        public DateTime? LastRepotted { get; set; }
    }
}