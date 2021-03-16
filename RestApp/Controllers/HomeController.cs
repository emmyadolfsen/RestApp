using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestApp.Data;

namespace RestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        //[HttpGet("/Start")]
        public async Task<IActionResult> Index()
        {
            ViewData["EatAt"] = HttpContext.Session.GetString("EatAt");


            // Hämta alla produkter - skicka med kategori
            var orderContext = _context.Product
                .Include(p => p.Category)
                .OrderByDescending(s => s.Category.CategoryName);

            // Returnera vy med resultat
            return View(await orderContext.ToListAsync());
        }

        // GET: Home/Admin
        public IActionResult Admin()
        {
            return View();
        }

        // GET: Home/ChooseTable
        public IActionResult ChooseTable()
        {
            // Skapa ny selectlista med bord
            ViewData["TableName"] = new SelectList(_context.Table, "Id", "TableName");
            return View();
        }


        // POST: Home/ChooseTable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChooseTable([Bind("TableName")] Table table)
        {
            // Gör en variabel med valt bord
            var EatAt = table.TableName;

            // Spara bordsnamn i en sessionsvariabel
            HttpContext.Session.SetString("EatAt", EatAt);

            // Gå till meny
            return RedirectToAction(nameof(Menu));
        }


        // GET: Home/SetTakeAway
        public IActionResult SetTakeAway()
        {
            // Gör en variabel med texten TakeAway
            string EatAt = "TakeAway";

            // Spara text i en sessionsvariabel
            HttpContext.Session.SetString("EatAt", EatAt);

            // Gå till meny
            return RedirectToAction(nameof(Menu));
        }


        // GET: Home/Menu
        [HttpGet("/Meny")]
        [HttpGet("/Home/Menu/{id}")]
        public async Task<IActionResult> Menu(int? id)
        {
            // Plocka ut alla kategorier
            var categories = _context.Category
                .OrderBy(i => i.CategoryName);

            // Lägg i en Viewbag för att loopa i vyn
            ViewBag.categories = categories;

            // Hämta ut bord eller takeaway
            var eatAt = HttpContext.Session.GetString("EatAt");

            // Om Take Away är valt
            if (eatAt == "TakeAway")
            {
                // Plocka ej ut kategori med alkohol
                var categoriesTA = _context.Category
                    .Where(o => o.Id != 1)
                    .Where(o => o.Id != 2)
                    .Where(o => o.Id != 3)
                    .Where(o => o.Id != 7)
                    .OrderBy(i => i.CategoryName);

                // Lägg i en Viewbag för att loopa i vyn
                ViewBag.categories = categoriesTA;
            }

            // Om id är medskickat 
            if (id != null)
            {
                // Plocka ut produkter med matchande kategoriId
                var category = from m in _context.Product
               .Where(p => p.CategoryId == id)
               .Include(p => p.Category)
                               select m;
                // Lägg i en Viewbag för att loopa i vyn
                string categoryName = categories.Where(x => x.Id == id).SingleOrDefault()?.CategoryName;
                ViewBag.categoryName = categoryName;

                // Returnera vy - skicka med resultat
                return View(await category.ToListAsync());
            }

            ViewBag.Sommartips = "Sommartips";
            // Hämta produkter - skicka med kategori
            var orderContext = _context.Product
                .Where(p => p.CategoryId == 1)
                .Include(p => p.Category).OrderByDescending(s => s.Category.CategoryName);
            // Returnera vy med resultat
            return View(await orderContext.ToListAsync());
        }


        // GET: Menu/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kolla om en session är startad
            if ((HttpContext.Session.GetString("orderNr")) == null)
            {
                // Hämta ut email från inloggning
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Email);
                var userEmail = claim.Value;

                // Hämta ut bord eller takeaway
                var eatAt = HttpContext.Session.GetString("EatAt");

                // Kolla om äta här eller takeaway är valt
                if (eatAt == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else if (eatAt != "TakeAway")
                {
                    // Starta ny order med email och bordsnummer
                    var order = new Order() { UserEmail = userEmail, Table = eatAt };
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                }
                else if (eatAt == "TakeAway")
                {
                    // Starta ny order med email och takeaway som true
                    var order = new Order() { UserEmail = userEmail, TakeAway = true };
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                }



                // Hämta ut senaste orderid där email matchar
                int orderid = _context.Order
                    .Where(p => p.UserEmail == userEmail).Max(u => u.Id);

                // Gör om till string - starta session med ordernr
                string obj = Convert.ToString(orderid);
                HttpContext.Session.SetString("orderNr", obj); // Lägg till objektet i session


                // Spara ändringar
                await _context.SaveChangesAsync();
            }

            // Sätt produkt id och order id
            int productId = (int)id;
            int orderId = Convert.ToInt32(HttpContext.Session.GetString("orderNr"));

            // Skapa ny orderitem
            var orderItem = new OrderItem() { OrderId = orderId, ProductId = productId };
            _context.Add(orderItem);
            await _context.SaveChangesAsync();

            // Hämta orderitems där ordernr matchar
            var orderItems = _context.OrderItem
                .Where(o => o.OrderId == orderId);

            // Plocka ut antal orderitems och gör om till string
            string OrderItemCount = orderItems.Count().ToString();
            // Sätt en sessionsvariabel för att skriva ut antal orderitems i "varukorgen"
            HttpContext.Session.SetString("OrderItemCount", OrderItemCount);

            // Hämta kategoriId för att komma tillbaks till rätt listning
            var categoryId = _context.Product
                .Where(o => o.Id == productId)
                .Include(o => o.Category)
                .FirstOrDefault();

            // Gå till meny
            return RedirectToAction("Menu", new { id = categoryId.CategoryId });

        }

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.categoryId = product.CategoryId;

            return View(product);
        }

        public async Task<IActionResult> UserOrder()
        {
            // Hämta ordernr från sessionsvariabel
            string obj = HttpContext.Session.GetString("orderNr");

            // Sätt viewdata till ordernr för utskrift i vy
            ViewData["checkOrderNr"] = obj;

            // Gör om ordernr till int, casta
            int orderNr = Convert.ToInt32(obj);

            // Hämta orderitems där ordernr matchar
            var orderItem = _context.OrderItem
                .Where(o => o.OrderId == orderNr)
                .Include(o => o.Order)
                .Include(o => o.Product)
                .OrderBy(o => o.ProductId);

            // Räkna ihop produkternas summor
            int totPrice = 0;
            foreach (var item in orderItem)
            {
                totPrice += item.Product.Price;
            }
            // Lägg in i viewdata för utskrift i vy
            ViewData["totPrice"] = totPrice;

            // Returnera produkter
            return View(await orderItem.ToListAsync());
        }

        public async Task<IActionResult> SendOrder()
        {
            // Hämta orderNr från sessionsvariabel
            int orderNr = Convert.ToInt32(HttpContext.Session.GetString("orderNr"));

            // Plocka ut order som matchar orderNr
            var changebool = _context.Order
                .Where(o => o.Id == orderNr)
                .FirstOrDefault();

            // Ändra bool och spara
            changebool.IsSent = true;
            await _context.SaveChangesAsync();

            // Rensa sessionvariabeln orderNr
            HttpContext.Session.Remove("orderNr");

            // Om takeaway session är startad
            if (HttpContext.Session.GetString("EatAt") == "TakeAway")
            {
                // Gå direkt till OrderDone för betalning
                return RedirectToAction("OrderDone", "OrderItem");
            }
            else if (HttpContext.Session.GetString("PendingOrder") == null) // Annars skapa en pending order
            {
                // Skapa text till sessionsvariabel
                var PendingOrder = "PendingOrder";
                // Lägg till i en sessionsvariabel
                HttpContext.Session.SetString("PendingOrder", PendingOrder);
            }

            // Gå till payorder
            return RedirectToAction(nameof(PayOrder));
        }

        [HttpGet("/Betala")]
        public async Task<IActionResult> PayOrder()
        {
            // Hämta ut email från inloggning
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Email);
            var userEmail = claim.Value;

            // Hämta ut order som är skickade men inte betalda och matchar email
            var orders = _context.Order
                .Where(o => o.UserEmail == userEmail)
                .Where(o => o.IsPayed == false)
                .Where(o => o.IsSent == true)
                .OrderByDescending(o => o.Id);
            // Skapa viewbag för att skriva ut i vyn
            ViewBag.orders = orders;

            // Lista orderitems där isSent är true - ispayed är false och email matchar
            var orderlist = _context.OrderItem
                .Where(o => o.Order.IsSent == true)
                .Where(o => o.Order.IsPayed == false)
                .Include(o => o.Order).Where(o => o.Order.UserEmail == userEmail)
                .Include(o => o.Product);

            // Räkna ihop produkternas summor
            int totalToPay = 0;
            foreach (var item in orderlist)
            {
                totalToPay += item.Product.Price;
            }
            // Skapa viewdata för utskrift i vy
            ViewData["totalToPay"] = totalToPay;

            // Returnera orderlist
            return View(await orderlist.ToListAsync());
        }

        [HttpGet("/Tack")]
        public IActionResult Payed()
        {
            // Rensa sessionsvariabler
            HttpContext.Session.Clear();

            return View();
        }

        // POST: OrderItem/Delete/5
        [HttpPost("/Radera")]
        public async Task<IActionResult> Delete(int id)
        {
            var orderItem = await _context.OrderItem.FindAsync(id);
            _context.OrderItem.Remove(orderItem);
            await _context.SaveChangesAsync();

            // Kör metoden checkitemcount för att ändra numret i "varukorgen"
            CheckItemCount();

            return RedirectToAction(nameof(UserOrder));
        }

        public void CheckItemCount()
        {
            // Hämta ordernr från sessionsvariabel
            int orderId = Convert.ToInt32(HttpContext.Session.GetString("orderNr"));
            // Hämta orderitems där ordernr matchar
            var orderItems = _context.OrderItem
                .Where(o => o.OrderId == orderId);

            // Plocka ut antal orderitems och gör om till string
            string OrderItemCount = orderItems.Count().ToString();
            // Sätt en sessionsvariabel för att skriva ut antal orderitems i "varukorgen"
            HttpContext.Session.SetString("OrderItemCount", OrderItemCount);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
