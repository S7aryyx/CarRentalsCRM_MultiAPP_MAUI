using System;

namespace MI_Cars.Models
{
    public class Rental
    {
        public int RentalId { get; set; }
        public int CarId { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal DailyRate { get; set; }
        public decimal TotalAmount => DailyRate * (decimal)(EndDate - StartDate).TotalDays;

        public bool IsBooking { get; set; } // 🔸 Флаг "бронь" для будущих дат
    }
}
