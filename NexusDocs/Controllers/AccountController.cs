using Microsoft.AspNetCore.Mvc;

namespace NexusDocs.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
