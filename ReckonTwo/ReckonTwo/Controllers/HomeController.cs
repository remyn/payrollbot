using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using ReckonTwo.Helpers;
using ReckonTwo.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace ReckonTwo.Controllers
{
    public class HomeController : Controller
    {
        private const string _botId = "rkn-payroll-bot";
        private const string _botSecretKey = "zkzhHeiTCjc.cwA.36s.6rsMSclswl2chHVZUIUbztcAwzvpFhx3Lk7reN5kJW0";

        TextToSpeechApiHelper api = new TextToSpeechApiHelper("bc9f4dfe324e474bb37b8f2480f1b7a4");

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
            await api.StartTextToSpeechAPI(api, text);

            return Json(new { status = "Success", message = "Success" });
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