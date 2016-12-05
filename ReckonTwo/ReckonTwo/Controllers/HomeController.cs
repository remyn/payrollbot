using System.Threading.Tasks;
using ReckonTwo.Helpers;
using System.Web.Mvc;

namespace ReckonTwo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> TextToSpeech()
        {
            var api = new TextToSpeechApiHelper("bc9f4dfe324e474bb37b8f2480f1b7a4");
            await api.StartTextToSpeechAPI(api);

            return View();
        }

        public ActionResult SpeechToText()
        {
            return View();
        }
    }
}