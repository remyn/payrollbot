using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Newtonsoft.Json;
using ReckonTwo.Models;

namespace ReckonTwo.Controllers
{
    public class NotificationController : SharedController
    {
        private static IList<NotificationModel> _notifications;

        public NotificationController()
        {
            _notifications = new List<NotificationModel>
            {
                new NotificationModel
                {
                    Id = 1,
                    Title = "First React Notification",
                    Text = "Ut enim ad minim veniam, quis nostrud exercitation...",
                    IsSeen = true,
                    IsStarred = false,
                    Status = "payroll",
                    NotificationTime = "Febraury 13, 2016"
                },
                new NotificationModel
                {
                    Id = 2,
                    Title = "Hey! A new Notification",
                    Text = "Ut enim ad minim veniam, quis nostrud exercitation...",
                    IsSeen = false,
                    IsStarred = true,
                    Status = "payroll",
                    NotificationTime = "Febraury 13, 2016"
                },
                new NotificationModel
                {
                    Id = 3,
                    Title = "Hey! A new Notification",
                    Text = "Ut enim ad minim veniam, quis nostrud exercitation...",
                    IsSeen = true,
                    IsStarred = false,
                    Status = "core",
                    NotificationTime = "Febraury 13, 2016"
                },
                new NotificationModel
                {
                    Id = 4,
                    Title = "Hey! A new Notification",
                    Text = "Ut enim ad minim veniam, quis nostrud exercitation...",
                    IsSeen = true,
                    IsStarred = true,
                    Status = "system",
                    NotificationTime = "Febraury 13, 2016"
                },
                new NotificationModel
                {
                    Id = 5,
                    Title = "Hey! A new Notification",
                    Text = "Ut enim ad minim veniam, quis nostrud exercitation...",
                    IsSeen = false,
                    IsStarred = false,
                    Status = "payroll",
                    NotificationTime = "Febraury 13, 2016"
                }
            };    
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public async Task<JsonResult> Notifications()
        {
            var response = await GetExternalResponse();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private static async Task<NotificationComponentModel> GetExternalResponse()
        {
            try
            {
                const string address = "https://sjpkykdgyj.execute-api.us-west-2.amazonaws.com/PayrollNotificationStage";

            var client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(address);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            var notifications = JsonConvert.DeserializeObject<List<NotificationModel>>(result);

            foreach (var model in notifications)
            {
                model.NotificationTime = "Febraury 13, 2016";
                model.Status = model.Status.ToLower();
            }

                return new NotificationComponentModel
                {
                    Notifications = notifications.Take(5),
                    NumberOfNewNotifications = notifications.Count(x => !x.IsSeen)
                };
            }
            catch (Exception e)
            {
                return new NotificationComponentModel
                {
                    Notifications = _notifications,
                    NumberOfNewNotifications = _notifications.Count(x => !x.IsSeen)
                };
            }
        }

    }
}