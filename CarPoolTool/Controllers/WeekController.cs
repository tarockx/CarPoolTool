using CarPoolTool.Models;
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
        private DateTime GetMonday(DateTime date)
        {
            bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = weekend ? date.AddDays(1) : date.AddDays(-1);
            }

            return date.Date;
        }

        // GET: Week
        public ActionResult Index()
        {
            DateTime today = DateTime.Today;
            DayOfWeek todaysDay = today.DayOfWeek;
            bool weekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;

            DateTime start = GetMonday(today);

            return RedirectToAction("Week", new { start = today.Date, activeDay = weekend ? DayOfWeek.Monday : todaysDay });
        }

        [HttpPost]
        public ActionResult Switch(string date)
        {
            DateTime start = DateTime.ParseExact(date, "yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture).Date;
            bool weekend = start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday;
            DayOfWeek startDay = start.DayOfWeek;
            start = GetMonday(start);

            return RedirectToAction("Week", new { start = start, activeDay = weekend ? DayOfWeek.Monday : startDay });
        }

        public ActionResult Week(DateTime start, DayOfWeek activeDay)
        {
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