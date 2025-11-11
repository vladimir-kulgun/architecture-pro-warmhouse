using System;
using System.Text.Json.Serialization;

namespace IOTServiceSubsriber
{
    public class TemperatureDataDto
    {
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime Timestamp { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        [JsonPropertyName("sensor_id")]
        public int SensorId { get; set; }
        [JsonPropertyName("sensor_type")]
        public string SensorType { get; set; }
        public string Description { get; set; }
    }
}
