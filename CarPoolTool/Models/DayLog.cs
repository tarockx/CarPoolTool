﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Models
{
    public class DayLog
    {
        public DateTime Date { get; set; }
        public Dictionary<User, UserStatus> Userdata { get; set; } = new Dictionary<User, UserStatus>();

        public DayLog() { }
        public DayLog(DateTime date)
        {
            Date = date;
        }

        public void InsertLog(CarpoolLog log)
        {
            if(log.driver == 0 && log.passenger == 0)
            {
                Userdata[log.User] = UserStatus.Absent;
            }
            else if(log.driver == 1)
            {
                Userdata[log.User] = UserStatus.Driver;
            }
            else
            {
                Userdata[log.User] = UserStatus.Passenger;
            }
        }

        public void FillMissing(IEnumerable<User> users)
        {
            foreach (var item in users)
            {
                if (!Userdata.ContainsKey(item))
                {
                    Userdata[item] = UserStatus.MissingData;
                }
            }
        }

        public List<User> GetByStatus(UserStatus status)
        {
            List<User> users = new List<User>();
            foreach (var key in Userdata.Keys)
            {
                if(Userdata[key] == status)
                {
                    users.Add(key);
                }
            }
            return users;
        }

        public string GetDriverStr()
        {
            var driver = GetByStatus(UserStatus.Driver).FirstOrDefault();
            return driver != null ? driver.display_name : "N/A";
        }

        public string GetPassengersStr()
        {
            return GetStr(UserStatus.Passenger);
        }

        public string GetAbsentStr()
        {
            return GetStr(UserStatus.Absent);
        }

        public string GetMissingStr()
        {
            return GetStr(UserStatus.MissingData);
        }

        private string GetStr(UserStatus status)
        {
            List<User> users = GetByStatus(status);
            if (users.Count == 0)
            {
                return "N/A";
            }
            string passengerStr = "";
            for (int i = 0; i < users.Count; i++)
            {
                passengerStr += users[i].display_name + (i < users.Count - 1 ? ", " : "");
            }
            return passengerStr;
        }
    }
}