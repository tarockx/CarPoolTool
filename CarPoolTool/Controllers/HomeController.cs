using CarPoolTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarPoolTool.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public String Index()
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var users = from a in entities.Users join b in entities.Totals on a.username equals b.username
                              orderby b.driver_total
                              select new { User = a, Total = b };
            string result = "Utenti attivi: </br>";
            foreach (var item in users)
            {
                result += item.User.display_name + " (Partecipato a " + item.Total.carpool_total.ToString() + " carpools, guidato in " + item.Total.driver_total.ToString() + " di essi)</br>";
            }
            return result;
        }
    }
}