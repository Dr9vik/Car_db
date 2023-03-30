using System;
using System.Collections.Generic;

namespace Data_Access_Layer.Common.Models
{
    public class CarDB
    {
        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }
        public virtual string NameNormal { get; set; }

        public virtual IList<UserCarDB> UserCars { get; set; }


        public virtual DateTimeOffset? TimeAdd { get; set; }
        public virtual DateTimeOffset? Modified { get; set; }
    }
}
