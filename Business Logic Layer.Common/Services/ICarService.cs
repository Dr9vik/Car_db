using Business_Logic_Layer.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Common.Services
{
    public interface ICarService
    {
        Task<CarBL> Create(CarBLCreate item);

        Task<CarBL> Update(CarBLUpdate item);

        Task Delete(Guid id);

        Task<CarBL> FindById(Guid id);

        Task<CarBL> FindByName(string name);

        Task<IList<CarBL>> FindAll();

        Task<IList<CarBL>> FindAll(int first, int second);
    }
}
