using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Business_Logic_Layer.Common.Models.Identity
{
    public class UserBL
    {
        public string Id { get; set; }
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset TokenExpires { get; set; }
        public IEnumerable<KeyValuePair<string,string>> Claims { get; set; }
    }

    public class UserBLCreate
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string PasswordOld { get; set; }
    }

    public class UserBLUpdate : UserBLCreate
    {
        public string Id { get; set; }
    }

    public class UserBLLogin
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
