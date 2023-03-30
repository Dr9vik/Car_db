using Business_Logic_Layer.Common.Models.Identity;
using System;

namespace Business_Logic_Layer.Common.Models
{
    public class UserCarBL
    {
        public virtual Guid Id { get; set; }

        public virtual string UserId { get; set; }

        public virtual UserBL User { get; set; }

        public virtual Guid CarId { get; set; }
        public virtual CarBL Car { get; set; }
    }
    public class UserCarBLCreate
    {
        public virtual string UserName { get; set; }
        public virtual Guid CarId { get; set; }
    }
}
