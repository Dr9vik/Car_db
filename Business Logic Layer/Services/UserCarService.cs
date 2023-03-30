using AutoMapper;
using Business_Logic_Layer.Common.Models;
using Business_Logic_Layer.Common.Services;
using Business_Logic_Layer.ExceptionModel;
using Data_Access_Layer.Common.Models;
using Data_Access_Layer.Common.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services
{
    public class UserCarService : IUserCarService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRepository2 _dataBase;
        private readonly IDapperRepository2 _dapper;
        private readonly IMapper _mapper;
        private readonly DateTimeOffset _dateTime;

        private readonly ILogger<UserCarService> _logger;
        public UserCarService(IRepository2 dataBase, UserManager<IdentityUser> userManager, IDapperRepository2 dapper, IMapper mapper, ILogger<UserCarService> logger)
        {
            _userManager = userManager;
            _dataBase = dataBase;
            _mapper = mapper;
            _dapper = dapper;
            _dateTime = DateTimeOffset.Now;
            _logger = logger;
        }

        public async Task<UserCarBL> Create(UserCarBLCreate item)
        {
            _logger.LogDebug("Create");

            var user = await _userManager.FindByNameAsync(item.UserName);
            if (string.IsNullOrWhiteSpace(user?.UserName))
                throw new UserArgumentException("Empty username");

            var searchResult = await _dataBase.Find<CarDB>(
               x => x.Id == item.CarId).FirstOrDefaultAsync().ConfigureAwait(false);
            if (searchResult == null)
                throw new ValidationException("Машина с таким именем не существует");


            UserCarDB result = new UserCarDB();
            result.Id = Guid.NewGuid();
            result.CarId = searchResult.Id;
            result.UserId = user.Id;
            result.TimeAdd = _dateTime;
            result.Modified = _dateTime;

            _dapper.Create(result);
            _dapper.Save();

            return _mapper.Map<UserCarBL>(result);
        }

        public async Task Delete(Guid id)
        {
            _logger.LogInformation("Delete");

            var searchResult = await _dataBase.Find<UserCarDB>(y => y.Id == id).FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (searchResult == null)
                throw new ValidationException("Отсутствует", null, null);

            var model = searchResult;

            _dataBase.Delete(searchResult);

            await _dataBase.SaveAsync().ConfigureAwait(false);
        }

        //желательно использовать Expression или функции сохраненные на базе
        public async Task<IList<UserCarBL>> FindAll()
        {
            var results = await _dapper.Sql<UserCarDB>("SELECT * FROM \"UserCar\"").ConfigureAwait(false);
            return _mapper.Map<IList<UserCarBL>>(results);
        }
        public async Task<IList<UserCarBL>> FindAll(int first, int second)
        {
            int limit = 5;
            int offcet = 0;
            if (second > first)
            {
                offcet = first;
                limit = second - first;
            }
            else if (first > second)
            {
                offcet = second;
                limit = first - second;
            }
            var results = await _dapper.Sql<UserCarDB>($"SELECT * FROM \"UserCar\" ORDER BY \"UserId\" OFFSET {offcet} LIMIT {limit}").ConfigureAwait(false);
            return _mapper.Map<IList<UserCarBL>>(results);
        }
    }
}
