using AutoMapper;
using Business_Logic_Layer.Common.Models;
using Data_Access_Layer.Common.Models;

namespace Business_Logic_Layer.Mappers
{
    public class CarProfile : Profile
    {
        public CarProfile()
        {
            CreateMap<CarBL, CarDB>()
                .ForMember(x => x.Modified, y => y.Ignore())
                .ForMember(x => x.NameNormal, y => y.Ignore())
                .ReverseMap();

            CreateMap<CarBLCreate, CarDB>();

            CreateMap<CarBLUpdate, CarDB>();
        }
    }
}
