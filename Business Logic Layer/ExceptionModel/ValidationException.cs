using Business_Logic_Layer.Common.Models.ModelExceptions;
using System;

namespace Business_Logic_Layer.ExceptionModel
{
    public class ValidationException : Exception
    {
        private ValidationME _model;
        public ValidationME Model
        {
            get
            {
                return _model;
            }
        }
        private object _loggerModel;
        public object LoggerModel
        {
            get
            {
                return _loggerModel;
            }
        }
        public ValidationException(string message, ValidationME returnModel = null, object loggerModel = null) : base(message)
        {
            _model = returnModel;
            _loggerModel = loggerModel;
        }
    }
}
