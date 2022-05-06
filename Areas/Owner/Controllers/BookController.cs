#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FPT_Book.Areas.Identity.Data;
using FPT_Book.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FPT_Book.Controllers
{

    [Area("Owner")]
    [Authorize(Roles = "Seller")]
    public class BookController : Controller
    {
        private readonly UserContext _context;

        private readonly UserManager<AppUser> _userManager;

        private readonly int iteminapage = 10;
        public BookController(UserContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Search and pagination

        [HttpGet]

        public async Task<IActionResult> Index(string searchString="", int id = 0)
        {
            ViewData["CurrentFilter"] = searchString;
            var userid = _userManager.GetUserId(HttpContext.User);
            var books = from s in _context.Book
                        select s;
            ViewBag.CurrentPage = id;
          
                books = books.Include(b => b.Category)
                .Include(s => s.Store)
                .ThenInclude(u => u.User)
                
                            
                    .Where(s => s.Title.Contains(searchString) || s.Category.Name.Contains(searchString) || s.Author.Contains(searchString))
                    .Where(b => b.Store.UId==userid);
                int numOfFilteredBook = books.Count();
                ViewBag.NumberOfPages = (int)Math.Ceiling((double)numOfFilteredBook / iteminapage);
                List<Book> booklist = await books.Skip(id * iteminapage)
                .Take(iteminapage).ToListAsync();
                return View(booklist);         
                              
        }

        // GET: Book/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Category)
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Book/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");

            var userid = _userManager.GetUserId(HttpContext.User);
           
            ViewData["StoreId"] = _context.Store.Where(s => s.UId == userid).FirstOrDefault().Name;
            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Isbn,Title,Pages,Author,Price,Desc,ImgUrl,CategoryId,StoreId")] Book book, IFormFile image)
        {
            var userid = _userManager.GetUserId(HttpContext.User);
            if (image != null)
            {
                string imgName = book.Isbn + Path.GetExtension(image.FileName);
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", imgName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                book.ImgUrl = "img/" + imgName;
                Book store = _context.Book.Include(b => b.Store).Where(s => s.Store.UId == userid).FirstOrDefault();
                book.StoreId = store.StoreId;
            }
            else
            {
                return View(book);
            }
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }            

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");

            ViewData["StoreId"] = _context.Store.Where(s => s.UId == userid).FirstOrDefault().Name;

            return View(book);
        }


        // GET: Book/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", book.CategoryId);
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // POST: Book/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Isbn,Title,Pages,Author,Price,Desc,ImgUrl,CategoryId,StoreId")] Book book)
        {
            if (id != book.Isbn)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Isbn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", book.CategoryId);
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // GET: Book/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Category)
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Book.FindAsync(id);
            _context.Book.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool BookExists(string id)
        {
            return _context.Book.Any(e => e.Isbn == id);
        }
    }
}
