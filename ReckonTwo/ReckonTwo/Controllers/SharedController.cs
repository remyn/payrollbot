using System;
using System.Web.Mvc;

namespace ReckonTwo.Controllers
{
    public class SharedController : Controller
    {
        public Guid GetLoggedInUserId()
        {
            try
            {
                return Guid.Parse(((System.Web.Security.FormsIdentity)Request.RequestContext.HttpContext.User.Identity).Ticket.UserData.Split('|')[1]);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        public string GetLoggedInUserFullName()
        {
            try
            {
                return ((System.Web.Security.FormsIdentity)Request.RequestContext.HttpContext.User.Identity).Ticket.UserData.Split('|')[0];
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}