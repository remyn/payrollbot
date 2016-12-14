using System;
using Newtonsoft.Json;

namespace ReckonTwo.Models
{
    public class NotificationModel
    {
        [JsonProperty("NotificationId")]
        public int Id { get; set; }

        [JsonProperty("CategoryName")]
        public string Status { get; set; }

        [JsonProperty("FlagRead")]
        public bool IsSeen { get; set; }

        public bool IsStarred { get; set; }

        //[JsonProperty("DateCreated")]
        public DateTime DateCreated { get; set; }
        public string NotificationTime { get; set; }
        public string Title { get; set; }

        [JsonProperty("Message")]
        public string Text { get; set; }
    }
}