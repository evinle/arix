using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArixBack.Models
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }

    }
    public enum LoginType
    {
        Normal = 0,
        Google = 1
        
    }
}