using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Helpers
{
    public class EntitiesHelper
    {
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
                daylog.FillMissingUsers(users, fillStatus.Value);
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
    }
}