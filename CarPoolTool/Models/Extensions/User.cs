﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Models
{
    public partial class User
    {
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return username.Equals(((User)obj).username);
        }
        
        public override int GetHashCode()
        {
            return username.GetHashCode();
        }

        public static User GetByUsername(string username)
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();
            return (from u in entities.Users where u.username.Equals(username) select u).FirstOrDefault();
        }

        public static List<User> GetAll()
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();
            return entities.Users.ToList();
        }
    }
}