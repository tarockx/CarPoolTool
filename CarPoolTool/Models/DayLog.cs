using System;
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
    }
}