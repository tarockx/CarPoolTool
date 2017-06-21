using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Helpers
{
    public class EntitiesHelper
    {
        public static IEnumerable<DayLog> GetWeek(DateTime start, int days)
        {
            return GetWeek(start, start.AddDays(days));
        }
        public static IEnumerable<DayLog> GetWeek(DateTime start, DateTime end)
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var log = from a in entities.CarpoolLogs
                      where a.data >= start && a.data < end
                      orderby a.data
                      select a;

            Dictionary<DateTime, DayLog> week = new Dictionary<DateTime, DayLog>();
            foreach (var item in log.ToList())
            {
                if (!week.ContainsKey(item.data.Date))
                {
                    week[item.data.Date] = new DayLog(item.data.Date);
                }
                week[item.data.Date].InsertLog(item);
            }

            //Fill missing users and/or days
            var users = entities.Users;
            DateTime curDay = start;
            while(curDay < end)
            {
                if (!week.ContainsKey(curDay))
                {
                    week[curDay] = new DayLog(curDay);
                }
                week[curDay].FillMissingUsers(users, UserStatus.MissingData, false);
                curDay = curDay.AddDays(1);
            }


            //Load alerts and holidays
            foreach (var daylog in week.Values)
            {
                var alerts = from a in entities.Alerts
                             where a.data == daylog.Date
                             select a;

                var holiday = (from h in entities.Holidays
                             where h.data == daylog.Date
                             select h).FirstOrDefault();

                daylog.Alerts = alerts.ToList();
                if(holiday != null)
                {
                    daylog.Holiday = holiday;
                }
            }
            

            return week.Values.OrderBy(x => x.Date);
        }

        public static void PersistDayLog(DayLog daylog, UserStatus? fillStatus)
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();
            PersistDayLog(daylog, entities, fillStatus, true);
        }

        public static void PersistDayLog(DayLog daylog, CarPoolToolEntities entities, UserStatus? fillStatus, bool persistChanges)
        {
            DateTime day = daylog.Date.Date;
            
            var users = entities.Users;
            if (fillStatus.HasValue)
            {
                daylog.FillMissingUsers(users, fillStatus.Value, true);
            }

            foreach (var user in users)
            {
                if (!daylog.Userdata.ContainsKey(user))
                {
                    continue;
                }

                var log = (from l in entities.CarpoolLogs where l.data == day && l.username == user.username select l).FirstOrDefault();
                UserStatus status = daylog.Userdata[user];

                //Remove entry
                if (status == UserStatus.MissingData)
                {
                    if (log != null)
                    {
                        entities.CarpoolLogs.Remove(log);
                    }
                }
                else
                {
                    if (log == null)
                    {
                        log = new CarpoolLog();
                        log.username = user.username;
                        log.data = day;
                        entities.CarpoolLogs.Add(log);
                    }

                    switch (status)
                    {
                        case UserStatus.Driver:
                            log.driver = 1;
                            log.passenger = 0;
                            break;
                        case UserStatus.Passenger:
                            log.driver = 0;
                            log.passenger = 1;
                            break;
                        case UserStatus.Absent:
                            log.driver = 0;
                            log.passenger = 0;
                            break;
                        default:
                            break;
                    }

                }
            }

            //Update o insert
            if (persistChanges)
            {
                entities.SaveChanges();
            }

        }


        public static void SetActive(User user, bool active)
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();
            var current = (from u in entities.Users where u.username == user.username select u).FirstOrDefault();
            current.active = (active ? (short)1 : (short)0);

            entities.SaveChanges();
        }
    }
}