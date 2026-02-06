using System.ComponentModel.DataAnnotations;

namespace WarsztatMVC.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa usługi jest wymagana")]
        [Display(Name = "Nazwa usługi")]
        public string Name { get; set; }

        [Display(Name = "Opis")]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        [Display(Name = "Cena (PLN)")]
        public decimal Price { get; set; }

        public List<Reservation>? Reservations { get; set; }
    }
}
