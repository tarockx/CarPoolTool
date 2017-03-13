using CarPoolTool.Helpers;
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
        private DateTime GetMonday(DateTime date, bool skipAheadIfWeekend)
        {
            bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = weekend && skipAheadIfWeekend ? date.AddDays(1) : date.AddDays(-1);
            }

            return date.Date;
        }

        private IEnumerable<DayLog> GetWeek(DateTime start)
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
            for (int i = 0; i < 5; i++)
            {
                DateTime curDay = start.AddDays(i);
                if (!week.ContainsKey(curDay))
                {
                    week[curDay] = new DayLog(curDay);
                }
                week[curDay].FillMissingUsers(users, UserStatus.MissingData);
            }


            //Load alerts
            foreach (var daylog in week.Values)
            {
                var alerts = from a in entities.Alerts
                             where a.data == daylog.Date
                             select a;
                daylog.Alerts = alerts.ToList();
            }

            return week.Values.OrderBy(x => x.Date);
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

            var week = GetWeek(start);

            ViewBag.ActiveDay = activeDay;
            return View("WeekView", week);
        }


        [HttpPost]
        public ActionResult Update(DateTime day, string username, UserStatus status, UserStatus? formerDriverStatus)
        {
            day = day.Date;
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var driver = (from l in entities.CarpoolLogs where l.data == day && l.driver == 1 select l).FirstOrDefault();
            if(driver != null)
            {
                if (formerDriverStatus.HasValue && formerDriverStatus != UserStatus.Driver)
                {
                    driver.driver = 0;
                    driver.passenger = formerDriverStatus == UserStatus.Absent ? (short)0 : (short)1;
                }
                else
                {
                    entities.CarpoolLogs.Remove(driver);
                }
                entities.SaveChanges();
            }

            var log = (from l in entities.CarpoolLogs where l.data == day && l.username == username select l).FirstOrDefault();

            //Remove entry
            if(status == UserStatus.MissingData)
            {
                if(log != null)
                {
                    entities.CarpoolLogs.Remove(log);
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

            }

            //Update o insert
            entities.SaveChanges();

            return RedirectToAction("Week", new { start = day.Date, skipAheadIfWeekend = false });
        }


        [HttpGet]
        public ActionResult WeekReset(DateTime start)
        {
            ViewBag.Section = ActiveSection.Week;

            if (start.DayOfWeek != DayOfWeek.Monday)
            {
                start = GetMonday(start, false);
            }
            DateTime end = start.AddDays(5);

            CarPoolToolEntities entities = new CarPoolToolEntities();

            var log = from a in entities.CarpoolLogs
                      where a.data >= start && a.data < end
                      select a;
            if(log != null)
            {
                entities.CarpoolLogs.RemoveRange(log);
                entities.SaveChanges();
            }

            return RedirectToAction("Week", new { start = start, skipAheadIfWeekend = false });
        }

        [HttpGet]
        public ActionResult DayEdit(DateTime day)
        {
            day = day.Date;
            DateTime start = day.Date;
            ViewBag.Section = ActiveSection.Week;

            if (start.DayOfWeek != DayOfWeek.Monday)
            {
                start = GetMonday(start, false);
            }

            var week = GetWeek(start);
            var daylog = from d in week where d.Date == day select d;

            return View("WeekEditView", daylog);
        }

        [HttpGet]
        public ActionResult WeekEdit(DateTime start)
        {
            ViewBag.Section = ActiveSection.Week;

            if (start.DayOfWeek != DayOfWeek.Monday)
            {
                start = GetMonday(start, false);
            }

            var week = GetWeek(start);

            return View("WeekEditView", week);
        }

        [HttpPost]
        public ActionResult WeekEdit(DateTime day, Dictionary<DayOfWeek, DayLog> weekdata)
        {
            ViewBag.Section = ActiveSection.Week;

            CarPoolToolEntities entities = new CarPoolToolEntities();

            foreach (var daylog in weekdata.Values)
            {
                EntitiesHelper.PersistDayLog(daylog, entities, UserStatus.Absent, false);
            }

            entities.SaveChanges();

            return RedirectToAction("Week", new { start = day, skipAheadIfWeekend = false });
        }

        [HttpPost]
        public ActionResult InsertAlert(Alert alert, DateTime start)
        {
            ViewBag.Section = ActiveSection.Week;

                CarPoolToolEntities entities = new CarPoolToolEntities();
                if (alert != null && alert.isValid())
                {
                    entities.Alerts.Add(alert);
                    entities.SaveChanges();
                }
            return RedirectToAction("Week", new { start = start, skipAheadIfWeekend = false });
        }

        [HttpGet]
        public ActionResult DeleteAlert(DateTime start, int alertId)
        {
            ViewBag.Section = ActiveSection.Week;

            if (start.DayOfWeek != DayOfWeek.Monday)
            {
                start = GetMonday(start, false);
            }

            if(alertId > 0)
            {
                CarPoolToolEntities entities = new CarPoolToolEntities();
                var alert = (from a in entities.Alerts where a.id == alertId select a).FirstOrDefault();
                if(alert != null) {
                    entities.Alerts.Remove(alert);
                    entities.SaveChanges();
                }
            }

            return RedirectToAction("Week", new { start = start, skipAheadIfWeekend = false });
        }
    }
}