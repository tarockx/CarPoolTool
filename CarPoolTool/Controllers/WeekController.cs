﻿using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPoolTool.Controllers
{
    public class WeekController : Controller
    {
        // GET: Week
        public ActionResult Index()
        {
            DateTime today = DateTime.Today;
            bool weekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;

            while(today.DayOfWeek != DayOfWeek.Monday)
            {
                today = weekend ? today.AddDays(1) : today.AddDays(-1);
            }

            return RedirectToAction("Week", new { start = today.Date });
        }


        public ActionResult Week(DateTime start)
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

            return View("WeekView", week.Values);
        }
    }
}