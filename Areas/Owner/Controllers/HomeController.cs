using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FPT_Book.Areas.Owner.Controllers
{
    public class HomeController : Controller
    {

        [Area("Owner")]
        [Authorize(Roles = "Seller")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
