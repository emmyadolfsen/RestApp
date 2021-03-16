using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestApp.Data;
using RestApp.Models;

namespace RestApp.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrderItem
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderItem.Include(o => o.Order).Include(o => o.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderItem/OrderList
        public async Task<IActionResult> OrderList()
        {
            // Lista orders som är skickade - isSent = true
            var orders = _context.Order
                .Where(o => o.IsSent == true)
                .OrderByDescending(o => o.Id);

            // Lägg i en viewbag för att skriva ut i en loop i vy
            ViewBag.orders = orders;


            // Lista orderitems där bool isSent är true
            var orderlist = _context.OrderItem
                .Where(o => o.Order.IsSent == true)
                .Include(o => o.Order)
                .Include(o => o.Product)
                .OrderBy(o => o.ProductId);

            // Returnera vy med orderlista(Lista med produkterna i en order)
            return View(await orderlist.ToListAsync());
        }

        // GET: OrderItem/OrderDone
        public async Task<IActionResult> OrderDone(int? id)
        {
            if (id == null) // Om id är medskickat
            {
                // Hämta ut email från inloggning
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Email);
                var userEmail = claim.Value;

                // Lista obetalade orders(IsPayed = false) där email matchar
                var changebool = _context.Order
                .Where(o => o.UserEmail == userEmail)
                .Where(o => o.IsPayed == false);

                // Loopa changebool listan och ändra bool till true
                foreach (var item in changebool)
                {
                    // Ändra bool och spara
                    item.IsPayed = true;
                    await _context.SaveChangesAsync();
                }

                // Gå till Payed sidan
                return RedirectToAction("Payed", "Home");
            }
            else
            {
                // Hämta ut order med id
                var changebool = _context.Order
                .Where(o => o.Id == id)
                .FirstOrDefault();

                // Ändra bool och spara
                changebool.IsPayed = true;
                await _context.SaveChangesAsync();
            }

            // Gå tillbaks till Orderlist
            return RedirectToAction(nameof(OrderList));
        }

        // GET: OrderItem/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Order, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Name");
            return View();
        }

        // POST: OrderItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,ProductId")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Order, "Id", "Id", orderItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Name", orderItem.ProductId);
            return View(orderItem);
        }

        // GET: OrderItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItem.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Order, "Id", "Id", orderItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Name", orderItem.Product.Name);
            return View(orderItem);
        }

        // POST: OrderItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductId")] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.Id))
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
            ViewData["OrderId"] = new SelectList(_context.Order, "Id", "Id", orderItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Id", orderItem.ProductId);
            return View(orderItem);
        }

        // GET: OrderItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItem
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItem.FindAsync(id);
            _context.OrderItem.Remove(orderItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItem.Any(e => e.Id == id);
        }
    }
}
