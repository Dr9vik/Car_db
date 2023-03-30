using AutoMapper;
using Business_Logic_Layer.Common.Models;
using Business_Logic_Layer.Common.Models.ModelExceptions;
using Business_Logic_Layer.Common.Services;
using Business_Logic_Layer.ExceptionModel;
using Data_Access_Layer.Common.Models;
using Data_Access_Layer.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public class CarService : ICarService
    {
        private readonly IRepository2 _dataBase;
        private readonly IDapperRepository2 _dapper;
        private readonly IMapper _mapper;
        private readonly DateTimeOffset _dateTime;

        private readonly ILogger<CarService> _logger;
        public CarService(IRepository2 dataBase, IDapperRepository2 dapper, IMapper mapper, ILogger<CarService> logger)
        {
            _dataBase = dataBase;
            _mapper = mapper;
            _dapper = dapper;
            _dateTime = DateTimeOffset.Now;
            _logger = logger;
        }

        public async Task<CarBL> Create(CarBLCreate item)
        {
            _logger.LogDebug("Create");

            var searchResult = await _dataBase.Find<CarDB>(
               x => x.Name.ToUpper() == item.Name.ToUpper()).FirstOrDefaultAsync().ConfigureAwait(false);
            CarBL returnModel;
            if (searchResult == null)
            {
                _logger.LogDebug("!searchResult.Any()");

                var result = _mapper.Map<CarDB>(item);
                result.Id = Guid.NewGuid();
                result.NameNormal = result.Name.ToUpper();
                result.TimeAdd = _dateTime;
                result.Modified = _dateTime;

                _dapper.Create(result);
                returnModel = _mapper.Map<CarBL>(result);
            }
            else
            {
                returnModel = _mapper.Map<CarBL>(searchResult);
                    throw new ValidationException("Машина с таким именем существует", new ValidationME2<CarBL>(returnModel));
            }

            _dapper.Save();

            return returnModel;
        }

        public async Task<CarBL> Update(CarBLUpdate item)
        {
            _logger.LogInformation("Update");

            var searchResult = await _dataBase.Find<CarDB>(
               x => x.Id == item.Id).FirstOrDefaultAsync()
               .ConfigureAwait(false);
            if (searchResult == null)
                throw new ValidationException("Машина отсутствует", null, item);

            var model = searchResult;

            searchResult = await _dataBase.Find<CarDB>(
               x => x.Name == item.Name && x.Id != item.Id).FirstOrDefaultAsync()
               .ConfigureAwait(false);
            if (searchResult != null)
            {
                var returnModel = _mapper.Map<CarBL>(searchResult);
                throw new ValidationException("Машина с таким именем существует", new ValidationME2<CarBL>(returnModel), searchResult);
            }

            model = _mapper.Map(item, model);
            model.NameNormal = model.Name.Trim().ToUpper();
            model.Modified = _dateTime;

            _dapper.Update(model);
            _dapper.Save();

            return _mapper.Map<CarBL>(model);
        }

        public async Task Delete(Guid id)
        {
            _logger.LogInformation("Delete");

            var searchResult = await _dataBase.Find<CarDB>(y => y.Id == id).FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (searchResult == null)
                throw new ValidationException("Машина отсутствует", null, null);

            var model = searchResult;

            _dataBase.Delete(searchResult);

            await _dataBase.SaveAsync().ConfigureAwait(false);
        }

        //желательно использовать Expression или функции на базе
        public async Task<CarBL> FindById(Guid id)
        {
            var results = await _dapper.Sql<CarDB>("SELECT * FROM \"Car\" WHERE \"Id\" = @id", new { id }).ConfigureAwait(false);
            return _mapper.Map<CarBL>(results.FirstOrDefault());
        }

        public async Task<CarBL> FindByName(string name)
        {
            var results = await _dapper.Sql<CarDB>("SELECT * FROM \"Car\" WHERE \"Name\" = @name", new { name }).ConfigureAwait(false);
            return _mapper.Map<CarBL>(results.FirstOrDefault());
        }
        public async Task<IList<CarBL>> FindAll()
        {
            var results = await _dapper.Sql<CarDB>("SELECT * FROM \"Car\"").ConfigureAwait(false);
            return _mapper.Map<IList<CarBL>>(results);
        }
        public async Task<IList<CarBL>> FindAll(int first, int second)
        {
            int limit = 5;
            int offcet = 0;
            if(second > first)
            {
                offcet = first;
                limit = second - first;
            }
            else if(first > second)
            {
                offcet = second;
                limit = first - second;
            }
            var results = await _dapper.Sql<CarDB>($"SELECT * FROM \"Car\" ORDER BY \"Name\" OFFSET {offcet} LIMIT {limit}").ConfigureAwait(false);
            return _mapper.Map<IList<CarBL>>(results);
        }
    }
}
