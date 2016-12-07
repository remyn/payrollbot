using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ReckonTwo.Helpers;
using ReckonTwo.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ReckonTwo.Controllers
{
    public class HomeController : Controller
    {
        private const string _botId = "rkn-payroll-bot";
        private const string _botSecretKey = "zkzhHeiTCjc.cwA.36s.6rsMSclswl2chHVZUIUbztcAwzvpFhx3Lk7reN5kJW0";
        private const string _azureStorageEndpoint = "DefaultEndpointsProtocol=https;AccountName=mornestorageaccount;AccountKey=GIQOmCrdQslI9rAir4/Kajzr8UwZNkRGCn0TfG0rIY4GyvVgU3Ejci/88HgEIXMIUkVwB3bfUxvfbAH7DNfQ7w==";
        private const string _speechApiKey = "42d4e9b82b4e43108387e5458216ab00";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TextToSpeech()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ConvertTextToSpeech(string text)
        {
            TextToSpeechApiHelper _speechApi = new TextToSpeechApiHelper(_speechApiKey);

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
            return View();
        }

        public async Task<ActionResult> BotDirect(Chat model)
        {
            var chat = new Chat();

            chat = await TalkToTheBot(model.ChatMessage);
            return View(chat);
        }

        private async Task<Chat> TalkToTheBot(string paramMessage)
        {
            DirectLineClient client = new DirectLineClient(_botSecretKey);
            Conversation conversation = System.Web.HttpContext.Current.Session["conversation"] as Conversation;

            string watermark = System.Web.HttpContext.Current.Session["watermark"] as string;

            if (conversation == null)
            {
                conversation = client.Conversations.NewConversation();
            }

            Message message = new Message
            {
                FromProperty = User.Identity.Name,
                Text = paramMessage
            };

            await client.Conversations.PostMessageAsync(conversation.ConversationId, message);

            Chat chat = await ReadBotMessagesAsync(client, conversation.ConversationId, watermark);
            System.Web.HttpContext.Current.Session["conversation"] = conversation;
            System.Web.HttpContext.Current.Session["watermark"] = chat.watermark;
            return chat;
        }

        private async Task<Chat> ReadBotMessagesAsync(DirectLineClient client, string conversationId, string watermark)
        {
            Chat chat = new Chat();

            bool messageReceived = false;
            while (!messageReceived)
            {
                var messages = await client.Conversations.GetMessagesAsync(conversationId, watermark);
                watermark = messages?.Watermark;

                foreach (Message message in messages.Messages.Where(i => i.FromProperty == _botId))
                {
                    if (message.Text != null)
                    {
                        chat.ChatResponse += " " + message.Text.Replace("\n\n", "<br />");
                    }
                }

                messageReceived = true;
            }

            chat.watermark = watermark;
            return chat;
        }
    }
}