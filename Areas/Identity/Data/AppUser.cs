using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FPT_Book.Models;
using Microsoft.AspNetCore.Identity;

namespace FPT_Book.Areas.Identity.Data;

// Add profile data for application users by adding properties to the FPT_BookUser class
public class AppUser : IdentityUser
{
    public DateTime? DoB { get; set; }
    public string? Address { get; set; }
    public Store? Store { get; set; }
    public virtual ICollection<Order>? Orders { get; set; }
    public virtual ICollection<Cart>? Carts { get; set; }

}

