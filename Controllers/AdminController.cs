using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatCar.Data;
using WarsztatCar.Models.ViewModels;

namespace WarsztatCar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var prices = await _context.Reservations
                .Include(r => r.Service)
                .Where(r => r.Status != "Cancelled" && r.Service != null)
                .Select(r => r.Service!.Price)
                .ToListAsync();

            var stats = new DashboardViewModel
            {
                TotalReservations = await _context.Reservations.CountAsync(),

                PendingReservations = await _context.Reservations
                    .CountAsync(r => r.Status == "Pending"),

                TodayReservationsCount = await _context.Reservations
                    .CountAsync(r => r.Date.Date == today),

                TotalRevenue = prices.Sum(),

                TodayReservations = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Service)
                    .Where(r => r.Date.Date == today)
                    .OrderBy(r => r.Date)
                    .ToListAsync()
            };

            return View(stats);
        }
    }
}