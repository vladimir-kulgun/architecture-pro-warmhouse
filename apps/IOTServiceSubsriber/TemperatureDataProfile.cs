using AutoMapper;

namespace IOTServiceSubsriber
{
    public class TemperatureDataProfile : Profile
    {
        public TemperatureDataProfile()
        {
            CreateMap<TemperatureData, TemperatureDataDto>()
                .ReverseMap();
        }
    }
}
