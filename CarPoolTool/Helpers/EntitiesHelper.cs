using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Helpers
{
    public class EntitiesHelper
    {
        public static void PersistDayLog(DayLog daylog)
        {

        }

        public static void PersistDayLog(DayLog daylog, CarPoolToolEntities entities)
        {
            DateTime day = daylog.Date.Date;
            CarPoolToolEntities entities = new CarPoolToolEntities();
            var users = entities.Users;
            daylog.FillMissing(users);

            foreach (var user in users)
            {
                var log = (from l in entities.CarpoolLogs where l.data == day && l.User == user select l).FirstOrDefault();
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
            entities.SaveChanges();

        }
    }
}