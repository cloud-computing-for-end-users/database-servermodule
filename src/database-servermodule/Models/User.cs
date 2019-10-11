using System;
using System.Collections.Generic;

namespace database_servermodule.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
