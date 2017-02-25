﻿using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPoolTool.Controllers
{
    [Authorize]
    public class WeekController : Controller
    {
        private DateTime GetMonday(DateTime date, bool skipAheadIfWeekend)
        {
            bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = weekend && skipAheadIfWeekend ? date.AddDays(1) : date.AddDays(-1);
            }

            return date.Date;
        }

        // GET: Week
        public ActionResult Index()
        {
            ViewBag.Section = ActiveSection.Week;
            DateTime today = DateTime.Today;
            return Week(today, true);
        }

        public ActionResult Switch(string date)
        {
            ViewBag.Section = ActiveSection.Week;
            DateTime start = DateTime.ParseExact(date, "yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture).Date;
            return Week(start, false);
        }


        public ActionResult Week(DateTime start, bool skipAheadIfWeekend)
        {
            ViewBag.Section = ActiveSection.Week;

            DayOfWeek activeDay = start.DayOfWeek;
            bool weekend = activeDay == DayOfWeek.Saturday || activeDay == DayOfWeek.Sunday;
            if(start.DayOfWeek != DayOfWeek.Monday)
            {
                start = GetMonday(start, skipAheadIfWeekend);
            }
            if (weekend)
            {
                activeDay = skipAheadIfWeekend ? DayOfWeek.Monday : DayOfWeek.Friday;
            }

            CarPoolToolEntities entities = new CarPoolToolEntities();

            DateTime end = start.AddDays(5);

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
            for(int i = 0; i < 5; i++)
            {
                DateTime curDay = start.AddDays(i);
                if (!week.ContainsKey(curDay)){
                    week[curDay] = new DayLog(curDay);
                }
                week[curDay].FillMissing(users);
            }

            ViewBag.ActiveDay = activeDay;
            return View("WeekView", week.Values);
        }


        [HttpPost]
        public ActionResult Update(DateTime day, string username, UserStatus status)
        {
            day = day.Date;
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var log = (from l in entities.CarpoolLogs where l.data == day && l.username == username select l).FirstOrDefault();

            //Remove entry
            if(status == UserStatus.MissingData)
            {
                if(log != null)
                {
                    entities.CarpoolLogs.Remove(log);
                    entities.SaveChanges();
                }
            }
            else
            {
                if(log == null)
                {
                    log = new CarpoolLog();
                    log.username = username;
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

                //Update o insert
                entities.SaveChanges();
            }

            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}