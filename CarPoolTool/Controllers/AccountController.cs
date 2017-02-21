using CarPoolTool.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace CarPoolTool.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout");
            }

            return View("LoginView");
        }

        [HttpPost]
        public ActionResult Login(string user, string pass, string returnUrl)
        {
            CarPoolToolEntities entities = new CarPoolToolEntities();

            var validUsers = from u in entities.Users where u.username.Equals(user) && u.password.Equals(pass) select u;
            if (validUsers == null || validUsers.Count() == 0)
            {
                TempData["loginError"] = "Username sconosciuto o password non corretta.";
                return View("LoginView");
            }

            User validUser = validUsers.ToList().First();

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, validUser.username));
            var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties() { IsPersistent = true }, id);

            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut();

            return RedirectToAction("Index", "Home");
        }

        public static User GetCurrentUser()
        {
            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return Models.User.GetByUsername(System.Web.HttpContext.Current.User.Identity.Name);
            }
            else
            {
                return null;
            }
        }
    }
}