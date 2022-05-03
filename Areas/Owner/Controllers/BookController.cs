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

namespace FPT_Book.Controllers
{

    [Area("Owner")]
    [Authorize(Roles = "Seller")]
    public class BookController : Controller
    {
        private readonly UserContext _context;

        private readonly int _recordsPerPage = 4;
        public BookController(UserContext context)
        {
            _context = context;
        }

       /* [HttpGet]

        public async Task<IActionResult> Index(string? Booksearch, int id = 0) 
        {
            ViewData["Getbookdetails"] = Booksearch;

            var bookquery = from b in _context.Book.Include(b => b.Category).Include(b => b.Store) select b;
            if (!String.IsNullOrEmpty(Booksearch))
            {
                bookquery = bookquery.Where(b => b.Title.Contains(Booksearch) || b.Author.Contains(Booksearch) *//*b.Category.Contains(Booksearch)*//*);

            }
            return View(await bookquery.AsNoTracking().ToListAsync());
        }*/

        public async Task<IActionResult> Search(string? searchString, int id = 0)
        {
            ViewData["CurrentFilter"] = searchString;
            var books = from b in _context.Book
                        .Include(b => b.Category)
                        .Include(b => b.Store)
                        select b;
            books = books.Where(b => b.Title.Contains(searchString));
            int numberOfRecords = books.Count();
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPerPage);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.numberOfRecords = numberOfRecords;
            if (numberOfRecords > 0)
            {
                int max = 5;
                int min;
                int end;
                if (numberOfRecords < max)
                {
                    min = 1;
                    end = numberOfRecords;
                }
                else
                {
                    min = id;
                    end = id + max - 1;
                    if (end > numberOfRecords)
                    {
                        end = numberOfRecords;
                    }
                }
                ViewBag.max = max;
                ViewBag.min = min;
                ViewBag.end = end;
            }
            List<Book> booksList = await books
                .Skip(id * _recordsPerPage)
                .Take(_recordsPerPage)
                .Include(b => b.Category)
                .Include(b => b.Store)
                .ToListAsync();
            ViewBag.EndPage = numberOfPages - 1;
            ViewBag.currentPage = id;
            return View(books);
        }

        // GET: Book
        public async Task<IActionResult> Index(int id = 0)
        {
            

            int numberOfRecords = await _context.Book.CountAsync();     //Count SQL
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPerPage);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            List<Book> listbook = await _context.Book.Include(b => b.Category).Include(b => b.Store)
             .Skip(id * _recordsPerPage)  //Offset SQL
             .Take(_recordsPerPage)       //Top SQL
             .ToListAsync();
            return View(listbook);
            /*var userContext = _context.Book.Include(b => b.Category).Include(b => b.Store);
            return View(await userContext.ToListAsync());*/
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
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Name");
            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Isbn,Title,Pages,Author,Price,Desc,ImgUrl,CategoryId,StoreId")] Book book, IFormFile image)
        {
            if (image != null)
            {
                string imgName = book.Isbn + Path.GetExtension(image.FileName);
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", imgName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                book.ImgUrl = imgName;
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", book.CategoryId);
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Name", book.StoreId);
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
