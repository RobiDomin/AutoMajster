using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WarsztatCar.Data;
using WarsztatCar.Models;
using WarsztatMVC.Models;
using WarsztatCar.Services;

namespace WarsztatCar.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public ReservationsController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Reservations
        public async Task<IActionResult> Index(string searchString, DateTime? searchDate, string searchStatus)
        {
            var reservations = _context.Reservations
                .Include(r => r.Service)
                .Include(r => r.User)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                reservations = reservations.Where(r => r.UserId == userId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                reservations = reservations.Where(r => r.User.UserName.Contains(searchString));
            }

            if (searchDate.HasValue)
            {
                reservations = reservations.Where(r => r.Date.Date == searchDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(searchStatus))
            {
                reservations = reservations.Where(r => r.Status == searchStatus);
            }

            reservations = reservations.OrderByDescending(r => r.Date);

            ViewData["CurrentFilter"] = searchString;
            ViewData["DateFilter"] = searchDate?.ToString("yyyy-MM-dd");
            ViewData["StatusFilter"] = searchStatus;

            return View(await reservations.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Service)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name");
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,ServiceId")] Reservation reservation)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            reservation.UserId = userId;

            reservation.Status = "Pending";

            bool terminZajety = await _context.Reservations
                .AnyAsync(r => r.Date == reservation.Date && r.Status != "Cancelled");

            if (terminZajety)
            {
                ModelState.AddModelError("Date", "Ten termin jest już zajęty! Wybierz inną godzinę.");
            }

            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Service");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();

                try
                {
                    var userEmail = User.Identity.Name;

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        await _emailSender.SendEmailAsync(
                            userEmail,
                            "Potwierdzenie rezerwacji - WarsztatCar",
                            $"<h3>Dziękujemy za rezerwację!</h3><p>Twoja wizyta została zaplanowana na dzień: <b>{reservation.Date}</b>.</p><p>Status: Oczekiwanie na potwierdzenie.</p>"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd wysyłki maila: " + ex.Message);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (reservation.UserId != userId) return Forbid();
            }

            List<SelectListItem> statusy = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Oczekiwanie" },
                new SelectListItem { Value = "Confirmed", Text = "Potwierdzono" },
                new SelectListItem { Value = "Completed", Text = "Wykonano" },
                new SelectListItem { Value = "Cancelled", Text = "Anulowano" }
            };

            ViewData["StatusList"] = new SelectList(statusy, "Value", "Text", reservation.Status);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);

            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Status,UserId,ServiceId")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("Service");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", reservation.UserId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Service)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAvailability(DateTime date)
        {
            bool isTaken = await _context.Reservations
                .AnyAsync(r => r.Date == date && r.Status != "Cancelled");

            return Json(new { isTaken = isTaken });
        }

        // GET: Reservations/Calendar
        public IActionResult Calendar()
        {
            return View();
        }

        // API dla FullCalendar
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Reservations
                .Where(r => r.Status != "Cancelled")
                .Select(r => new
                {
                    title = "Zajęty",
                    start = r.Date.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.Date.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#546e7a",
                    textColor = "#ffffff"
                })
                .ToListAsync();

            return Json(events);
        }
    }
}