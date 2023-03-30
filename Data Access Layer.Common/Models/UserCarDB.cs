using System;

namespace Data_Access_Layer.Common.Models
{
    public class UserCarDB
    {
        public virtual Guid Id { get; set; }

        public virtual string UserId { get; set; }

        public virtual Guid CarId { get; set; }
        public virtual CarDB Car { get; set; }

        public virtual DateTimeOffset? TimeAdd { get; set; }
        public virtual DateTimeOffset? Modified { get; set; }
    }
}
