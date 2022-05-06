using System.ComponentModel.DataAnnotations;

namespace FPT_Book.Models
{
    public class Book
    {
        [Key]
        public string Isbn { get; set; } = null!;
        public string Title { get; set; }
        public int Pages { get; set; }
        public string Author { get; set; }
        public double Price { get; set; }
        public string Desc { get; set; }
        public string? ImgUrl { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int? StoreId { get; set; }
        public Store? Store { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }

        //OLD code
        public virtual ICollection<Cart>? Carts { get; set; }
    /*    public string Id { get; internal set; }*/

        internal static void Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
