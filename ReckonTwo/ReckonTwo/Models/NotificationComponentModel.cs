using System.Collections.Generic;

namespace ReckonTwo.Models
{
    public class NotificationComponentModel
    {
        public IEnumerable<NotificationModel> Notifications { get; set; }
        public int NumberOfNewNotifications { get; set; }
    }
}