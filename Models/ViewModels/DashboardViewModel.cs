using WarsztatCar.Models;
using WarsztatMVC.Models;

namespace WarsztatCar.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int TodayReservationsCount { get; set; }
        public decimal TotalRevenue { get; set; }  

        public List<Reservation> TodayReservations { get; set; } = new List<Reservation>();
    }
}