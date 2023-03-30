using System;

namespace Business_Logic_Layer.Common.Models.Identity
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string TokenString { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
