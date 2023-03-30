using AutoMapper;
using Business_Logic_Layer.Common.Models;
using Data_Access_Layer.Common.Models;

namespace Business_Logic_Layer.Mappers
{
    public class UserCarProfile : Profile
    {
        public UserCarProfile()
        {
            CreateMap<UserCarBL, UserCarDB>()
                .ForMember(x => x.Modified, y => y.Ignore())
                .ReverseMap();
        }
    }
}
