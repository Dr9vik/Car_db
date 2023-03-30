using Business_Logic_Layer.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Common.Services
{
    public interface IUserCarService
    {
        Task<UserCarBL> Create(UserCarBLCreate item);

        Task Delete(Guid id);

        Task<IList<UserCarBL>> FindAll();
        Task<IList<UserCarBL>> FindAll(int first, int second);
    }
}
