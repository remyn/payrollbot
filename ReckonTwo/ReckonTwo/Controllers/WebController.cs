using ReckonTwo.Models;
using ReckonTwo.Security;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ReckonTwo.Controllers
{
    public class WebController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LogInModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "Email,Password")] LogInModel user)
        {
            if (ModelState.IsValid)
            {
                var encPass = Helper.HashPassword(user.Password.Trim());

                if (!Helper.ConfirmLoginDetails(encPass, user.Email.Trim()))
                {
                    ViewBag.ErrorMessage = "Email or Password is incorrect. Please try again.";
                    return View(user);
                }

                User loggedInUser = new User
                {
                    UserID = Guid.NewGuid(),
                    Email = user.Email.Trim(),
                    FirstName = "Joe",
                    LastName = "Bloggs",
                    FlagAdmin = true,
                    FlagDeleted = false,
                    Password = encPass
                };

                DateTime cookieIssuedDate = DateTime.Now;

                var ticket = new FormsAuthenticationTicket(0,
                                 loggedInUser.Email,
                                 cookieIssuedDate,
                                 cookieIssuedDate.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                                 false,
                                 string.Concat(loggedInUser.FullName, "|", loggedInUser.UserID.ToString()),
                                 FormsAuthentication.FormsCookiePath);

                string encryptedCookieContent = FormsAuthentication.Encrypt(ticket);

                var formsAuthenticationTicketCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedCookieContent)
                {
                    Domain = FormsAuthentication.CookieDomain,
                    Path = FormsAuthentication.FormsCookiePath,
                    HttpOnly = true,
                    Secure = false
                };

                System.Web.HttpContext.Current.Response.Cookies.Add(formsAuthenticationTicketCookie);

                return RedirectToAction("Index", "Home", null);
            }

            return View(new LogInModel());
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Web");
        }

    }
}