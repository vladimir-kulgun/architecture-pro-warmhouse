using System.Threading.Tasks;

namespace IOTServiceSubsriber
{
    public interface ITemperatureClient
    {
        Task<TemperatureDataDto> GetTemperatureAsync(int sensorId);
    }
}