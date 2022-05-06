#nullable disable
using FPT_Book.Areas.Identity.Data;
using FPT_Book.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FPT_Book.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserContext _context;
        private readonly int iteminapage = 12;

        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender, UserManager<AppUser> userManager, UserContext context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "Customer")]
        public IActionResult ForCustomerOnly()
        {
            ViewBag.message = "This is for Customer only! Hi " + _userManager.GetUserName(HttpContext.User);
            return View("Views/Home/Index.cshtml");
        }

        [Authorize(Roles = "Seller")]
        public IActionResult ForSellerOnly()
        {
            ViewBag.message = "This is for Store Owner only!";
            return View("Areas/Owner/Views/Home/Index.cshtml");
        }

        public async Task<IActionResult> Index(string searchString = "", int id = 0)
        {
            ViewData["CurrentFilter"] = searchString;

            var books = from s in _context.Book
                        select s;
            if (searchString != null)
            {
                books = books.Include(b => b.Category).
                    Where(s => s.Title.Contains(searchString) || s.Category.Name.Contains(searchString) || s.Author.Contains(searchString));
            }
            int numOfFilteredBook = books.Count();
            ViewBag.NumberOfPages = (int)Math.Ceiling((double)numOfFilteredBook / iteminapage);
            ViewBag.CurrentPage = id;
            List<Book> List = await books.Skip(id * iteminapage)
                .Take(iteminapage).ToListAsync();
            if (id > 1)
            {
                ViewBag.idpagprev = id - 1;
            }
            ViewBag.idpagenext = id + 1;

            ViewBag.currentPage = id;
            return View(List);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookdetails = await _context.Book
                .Include(b => b.Category)
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (bookdetails == null)
            {
                return NotFound();
            }

            return View(bookdetails);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}