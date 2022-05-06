#nullable disable
using FPT_Book.Areas.Identity.Data;
using FPT_Book.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList;

namespace FPT_Book.Controllers
{
    public class TestListController : Controller
    {
        private readonly ILogger<TestListController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserContext _context;
        

        public TestListController(ILogger<TestListController> logger, IEmailSender emailSender, UserManager<AppUser> userManager, UserContext context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            int pageSize = 10;


            /* var books = from s in _context.Book
                         select s;*/
            var onePageOfBooks = _context.Book.ToPagedList(pageNumber, pageSize);

           /* List<Book> testList = await books.ToListAsync();*/
            return View(onePageOfBooks);
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