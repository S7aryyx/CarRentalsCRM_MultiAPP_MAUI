namespace MI_Cars.Models
{
    public class CarDate
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } // "Free", "Booked", "Rented"
        public Rental Rental { get; set; } // null, если свободно
    }
}
