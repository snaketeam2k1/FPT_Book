using FPT_Book.Areas.Identity.Data;
using FPT_Book.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPT_Book.Controllers
{
	public class CartsController : Controller
	{
        private readonly UserContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CartsController(UserContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ActionResult BackToLogin()
        {            
            return View();
        }

        public async Task<IActionResult> AddToCart(string isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);

            if (thisUserId == null)
            {
                return RedirectToAction("BackToLogin", "Carts"/*, new { area ="" }*/);

            }

            Cart myCart = new Cart() { 
                UId = thisUserId, 
                BookIsbn = isbn,
                Quantity = 1
            };
            Cart fromDb = _context.Cart.FirstOrDefault(c => c.UId == thisUserId && c.BookIsbn == isbn);
            //if not existing (or null), add it to cart. If already added to Cart before, ignore it.
            if (fromDb == null)
            {
                _context.Add(myCart);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            List<Cart> myDetailsInCart = await _context.Cart
                .Where(c => c.UId == thisUserId)
                .Include(c => c.Book)
                .ToListAsync();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    //Step 1: create an order
                    Order myOrder = new Order();
                    myOrder.UId = thisUserId;
                    myOrder.OrderDate = DateTime.Now;
                    myOrder.Total = myDetailsInCart.Select(c => c.Book.Price)
                        .Aggregate((c1, c2) => c1 + c2);
                    _context.Add(myOrder);
                    await _context.SaveChangesAsync();

                    //Step 2: insert all order details by var "myDetailsInCart"
                    foreach (var item in myDetailsInCart)
                    {
                        OrderDetail detail = new OrderDetail()
                        {
                            OrderId = myOrder.Id,
                            BookIsbn = item.BookIsbn,
                            Quantity = 1
                        };
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();

                    //Step 3: empty/delete the cart we just done for thisUser
                    _context.Cart.RemoveRange(myDetailsInCart);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error occurred in Checkout" + ex);
                }
            }
            return RedirectToAction("Invoice", "Carts");
        }

        public IActionResult Invoice()
        {          
            return View();
        }

        public ActionResult Index()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            return View(_context.Cart.Include(b => b.Book).Where(c => c.UId == thisUserId));
        }

    }
}
