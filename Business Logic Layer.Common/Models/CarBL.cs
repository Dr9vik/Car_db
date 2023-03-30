using System;
using System.Collections.Generic;

namespace Business_Logic_Layer.Common.Models
{
    public class CarBL
    {
        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }

        public virtual IList<UserCarBL> UserCars { get; set; }


        public virtual DateTimeOffset? TimeAdd { get; set; }
        public virtual DateTimeOffset? Modified { get; set; }
    }
    public class CarBLCreate
    {
        public virtual string Name { get; set; }
    }
    public class CarBLUpdate : CarBLCreate
    {
        public virtual Guid Id { get; set; }
    }
}
