using System.Collections.ObjectModel;

namespace MI_Cars.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public string Color { get; set; }

        public ObservableCollection<Rental> RentalsForCar { get; set; } = new();
        public ObservableCollection<CarDate> DatesForCar { get; set; } = new();
    }
}
