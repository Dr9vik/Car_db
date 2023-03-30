using Business_Logic_Layer.ExceptionModel;
using Data_Access_Layer.ExceptionModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ServiceStack.Text;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace car_db.ConfiguringApps
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (DAException ex)
            {
                _logger.LogCritical(ex, ex.Message);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is SecurityTokenException)
            {
                await ResponseAsync(context.Response, HttpStatusCode.Unauthorized);
            }
            catch (UserArgumentException ex)
            {
                _logger.LogWarning(ex, "{model}", ex.Model);
                await ResponseAsync(context.Response, HttpStatusCode.Unauthorized, "Invalid token");
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "{model}", ex.Model);
                await ResponseAsync(context.Response, HttpStatusCode.BadRequest, ex.Model);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Uncaught Exception");
                await ResponseAsync(context.Response, HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        private static async Task ResponseAsync(HttpResponse response, HttpStatusCode statusCode, object? errorModel = null)
        {
            response.Clear();
            response.StatusCode = (int)statusCode;

            if (errorModel != null)
            {
                response.ContentType = "application/json";
                await response.WriteAsync(JsonConvert.SerializeObject(errorModel));
            }
        }

    }
}
