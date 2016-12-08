using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ReckonTwo.Helpers;
using ReckonTwo.Models;
using ReckonTwo.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

namespace ReckonTwo.Controllers
{
    public class HomeController : SharedController
    {
        private const string _botId = "rkn-payroll-bot";
        private const string _botSecretKey = "zkzhHeiTCjc.cwA.36s.6rsMSclswl2chHVZUIUbztcAwzvpFhx3Lk7reN5kJW0";
        private const string _azureStorageEndpoint = "DefaultEndpointsProtocol=https;AccountName=mornestorageaccount;AccountKey=GIQOmCrdQslI9rAir4/Kajzr8UwZNkRGCn0TfG0rIY4GyvVgU3Ejci/88HgEIXMIUkVwB3bfUxvfbAH7DNfQ7w==";
        private const string _speechApiKey = "42d4e9b82b4e43108387e5458216ab00";
        private static IList<CommentModel> _comments;
        private static SpeechApiHelper _speechApi;

        public ActionResult Index()
        {
            _comments = new List<CommentModel>();
            _speechApi = new SpeechApiHelper(_speechApiKey);

            var userGuid = GetLoggedInUserID();
            var userFullName = GetLoggedInUserFullName();

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
        public async Task<ActionResult> AddComment(CommentModel comment)
        {
            comment.Id = _comments.Count + 1;
            comment.IsBot = false;
            comment.MessageTime = DateTime.Now.ToString("HH:mm tt");
            _comments.Add(comment);

            var botAnswer = await AddBotAnswer(comment.Text);

            return Json(botAnswer);
        }

        private async Task<string> AddBotAnswer(string message)
        {
            var botAnswer = new CommentModel
            {
                Id = _comments.Count + 1,
                Author = "Reckon Bot",
                Text = string.Empty,
                MessageTime = DateTime.Now.ToString("HH:mm tt"),
                IsBot = true
            };

            var chat = await TalkToTheBot(message);

            botAnswer.Text = chat.ChatResponse;

            _comments.Add(botAnswer);

            return botAnswer.Text;
        }

        [HttpPost]
        public async Task<ActionResult> ConvertTextToSpeech(string text)
        {
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

                return RedirectToAction("index", "home", null);
            }

            return View(new LogInModel());
        }

        public ActionResult LogOff()
        {
            _comments = null;
            FormsAuthentication.SignOut();

            return RedirectToAction("login", "home");
        }
    }
}