using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ReckonTwo.Helpers;
using ReckonTwo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace ReckonTwo.Controllers
{
    public class HomeController : Controller
    {
        private const string _botId = "rkn-payroll-bot";
        private const string _botSecretKey = "zkzhHeiTCjc.cwA.36s.6rsMSclswl2chHVZUIUbztcAwzvpFhx3Lk7reN5kJW0";
        private const string _azureStorageEndpoint = "DefaultEndpointsProtocol=https;AccountName=mornestorageaccount;AccountKey=GIQOmCrdQslI9rAir4/Kajzr8UwZNkRGCn0TfG0rIY4GyvVgU3Ejci/88HgEIXMIUkVwB3bfUxvfbAH7DNfQ7w==";
        private const string _speechApiKey = "42d4e9b82b4e43108387e5458216ab00";
        private static readonly IList<CommentModel> _comments = new List<CommentModel>();

        //static HomeController()
        //{
        //    _comments = new List<CommentModel>
        //    {
        //        new CommentModel
        //        {
        //            Id = 1,
        //            Author = "Daniel Lo Nigro",
        //            Text = "Hello ReactJS.NET World!",
        //            MessageTime = "10:05 a.m.",
        //            IsBot = false
        //        },
        //        new CommentModel
        //        {
        //            Id = 2,
        //            Author = "Pete Hunt",
        //            Text = "This is one comment",
        //            MessageTime = "11:15 a.m.",
        //            IsBot = false
        //        },
        //        new CommentModel
        //        {
        //            Id = 3,
        //            Author = "Jordan Walke",
        //            Text = "This is *another* comment",
        //            MessageTime = "03:07 p.m.",
        //            IsBot = false
        //        }
        //    };
        //}

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TextToSpeech()
        {
            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }

        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Comments()
        {
            return Json(_comments, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddComment(CommentModel comment)
        {
            // Create a fake ID for this comment
            comment.Id = _comments.Count + 1;
            comment.IsBot = false;
            comment.MessageTime = DateTime.Now.ToString("HH:mm tt");
            _comments.Add(comment);
            return Content("Success :)");
        }

        [HttpPost]
        public async Task<ActionResult> AddBotAnswer(string message)
        {
            var botAnswer = new CommentModel
            {
                Id = _comments.Count + 1,
                Author = "Reckon Bot",
                Text = string.Empty,
                MessageTime = DateTime.Now.ToString("HH:mm tt"),
                IsBot = true
            };

            //if (message.Equals("Hello", StringComparison.CurrentCultureIgnoreCase))
            //    botAnswer.Text = "Hey! Please ask me something :)";

            //if (message.Equals("How are you?", StringComparison.CurrentCultureIgnoreCase))
            //    botAnswer.Text = "I'm really good, what about you?";

            //if (botAnswer.Text.Equals(string.Empty))
            //    botAnswer.Text = "I don't have the answer for that question, please ask something different";


            var chat = await TalkToTheBot(message);

            botAnswer.Text = chat.ChatResponse;

            _comments.Add(botAnswer);

            return Content("Success :)");
        }

        [HttpPost]
        public async Task<ActionResult> ConvertTextToSpeech(string text)
        {
            var _speechApi = new SpeechApiHelper(_speechApiKey);

            var audioStreamBytes = await _speechApi.StartTextToSpeechAPI(_speechApi, text);

            //for now, convert audio byte[] to an .mp3, store in an azure cloud blob and stream from there
            //in a real app we will live stream the audio to the browser. Easier said than done!
            var tempAudioFileName = string.Concat("tempAI", new Random().Next(), ".mp3");
            var tempAudioFilePath = HttpContext.Server.MapPath("~/" + tempAudioFileName);
            System.IO.File.WriteAllBytes(tempAudioFilePath, audioStreamBytes);

            //push to azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_azureStorageEndpoint);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("personalcontainer");

            //cleanup audio blobs
            foreach (var blob in blobContainer.ListBlobs())
            {
                blobContainer.GetBlockBlobReference(((CloudBlockBlob)blob).Name).DeleteIfExists();
            }

            //write new audio to a blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(tempAudioFileName);
            using (var fileStream = System.IO.File.OpenRead(tempAudioFilePath))
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }

            var blobUri = "";
            foreach (var blobItem in blobContainer.ListBlobs())
            {
                blobUri = blobItem.Uri.ToString();
            }

            //cleanup
            System.IO.File.Delete(tempAudioFilePath);

            return Json(blobUri, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SpeechToText()
        {
            //var _speechApi = new SpeechApiHelper(_speechApiKey);
            //var resultText = await _speechApi.StartSpeechToTextAPI(_speechApi, new byte[]);

            return View();
        }

        public async Task<ActionResult> BotDirect(Chat model)
        {
            var chat = await TalkToTheBot(model.ChatMessage);
            return View(chat);
        }

        private async Task<Chat> TalkToTheBot(string paramMessage)
        {
            var client = new DirectLineClient(_botSecretKey);
            var conversation = System.Web.HttpContext.Current.Session["conversation"] as Conversation;

            var watermark = System.Web.HttpContext.Current.Session["watermark"] as string;

            if (conversation == null)
            {
                conversation = client.Conversations.NewConversation();
            }

            var message = new Message
            {
                FromProperty = User.Identity.Name,
                Text = paramMessage
            };

            await client.Conversations.PostMessageAsync(conversation.ConversationId, message);

            var chat = await ReadBotMessagesAsync(client, conversation.ConversationId, watermark);
            System.Web.HttpContext.Current.Session["conversation"] = conversation;
            System.Web.HttpContext.Current.Session["watermark"] = chat.Watermark;
            return chat;
        }

        private async Task<Chat> ReadBotMessagesAsync(DirectLineClient client, string conversationId, string watermark)
        {
            var chat = new Chat();

            var messageReceived = false;
            while (!messageReceived)
            {
                var messages = await client.Conversations.GetMessagesAsync(conversationId, watermark);
                watermark = messages?.Watermark;

                foreach (var message in messages.Messages.Where(i => i.FromProperty == _botId))
                {
                    if (message.Text != null)
                    {
                        chat.ChatResponse += " " + message.Text.Replace("\n\n", "<br />");
                    }
                }

                messageReceived = true;
            }

            chat.Watermark = watermark;
            return chat;
        }
    }
}