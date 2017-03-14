using CarPoolTool.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CarPoolTool.Controllers
{
    public class AccountController : Controller
    {

        private String sha256(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public ActionResult Index()
        {
            ViewBag.Section = ActiveSection.Account;

            if (User.Identity.IsAuthenticated)
            {
                return View("UserView", UserTotal.Get(GetCurrentUser()));
            }
            else
            {
                return View("LoginView");
            }
        }

        // GET: Account
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Section = ActiveSection.Account;
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout");
            }

            return View("LoginView");
        }

        [HttpPost]
        public ActionResult Login(string user, string pass, string returnUrl)
        {
            ViewBag.Section = ActiveSection.Account;

            CarPoolToolEntities entities = new CarPoolToolEntities();

            string passHashStr = sha256(pass);

            var validUsers = from u in entities.Users where u.username.Equals(user) && u.password.Equals(passHashStr) select u;
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
                return RedirectToAction("Index");
            }
        }

        public ActionResult Logout()
        {
            ViewBag.Section = ActiveSection.Account;
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut();

            return RedirectToAction("Index");
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