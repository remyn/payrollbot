using ReckonTwo.Helpers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ReckonTwo.Controllers
{
    public class HomeController : Controller
    {
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
    }
}