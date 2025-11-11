using System;
using System.ComponentModel.DataAnnotations;

namespace MonitoringApi
{
    public class TemperatureDataEntity
    {
        [Key]
        public int Id { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime Timestamp { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public int SensorId { get; set; }
        public string SensorType { get; set; }
        public string Description { get; set; }
    }
}
