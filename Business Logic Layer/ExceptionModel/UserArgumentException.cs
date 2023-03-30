using System;

namespace Business_Logic_Layer.ExceptionModel
{
    public class UserArgumentException : Exception
    {
        public object Model { get; set; }

        public UserArgumentException(string message) : base(message)
        {

        }
        public UserArgumentException(string message, object model = null) : base(message)
        {
            Model = model;
        }
    }
}
