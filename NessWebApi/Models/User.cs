﻿using NessWebApi.Data;
namespace NessWebApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsConfirmed { get; set; }
        public string ImageUrl { get; set; }

    }
}