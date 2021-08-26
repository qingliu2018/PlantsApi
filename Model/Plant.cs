using System;

namespace PlantsApi.Model
{
    public class Plant : BaseModel
    {
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string WateringPeriodInDays { get; set; }
        public string PhotoUri { get; set; }
        public string WikipediaUri { get; set; }
    }
}