using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatCar.Data;

namespace WarsztatCar.Controllers
{
    [Route("api/[controller]")] // Adres to: /api/servicesapi
    [ApiController]
    public class ServicesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServicesApi
        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.Services
                .Select(s => new { s.Id, s.Name, s.Price, s.Description })
                .ToListAsync();

            return Ok(services);
        }
    }
}