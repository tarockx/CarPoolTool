using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPoolTool.Controllers
{
    public class UserStatsController : Controller
    {
        // GET: UserStats
        public ActionResult Index()
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var usertotals = from a in entities.Users
                             join b in entities.Totals on a.username equals b.username
                             orderby b.driver_total
                             select new UserTotal() { Total = b, User = a };

            return View("UserStatsView", usertotals.ToList());
        }
    }
}