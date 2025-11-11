using AutoMapper;

namespace MonitoringConsumer
{
    public class TemperatureDataProfile : Profile
    {
        public TemperatureDataProfile()
        {
            CreateMap<TemperatureData, TemperatureDataEntity>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
