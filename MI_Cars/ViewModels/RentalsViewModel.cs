using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MI_Cars.Models;
using MI_Cars.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MI_Cars.ViewModels
{
    public partial class RentalsViewModel : ObservableObject
    {
        private readonly PostgresService _service = new PostgresService();

        [ObservableProperty]
        ObservableCollection<Rental> rentals;

        public RentalsViewModel()
        {
            Rentals = new ObservableCollection<Rental>();

            LoadRentalsCommand = new AsyncRelayCommand(LoadRentalsAsync);
            AddRentalCommand = new AsyncRelayCommand(AddRentalAsync);

            // Загружаем данные сразу
            _ = LoadRentalsAsync();
        }

        public IAsyncRelayCommand LoadRentalsCommand { get; }
        public IAsyncRelayCommand AddRentalCommand { get; }

        private async Task LoadRentalsAsync()
        {
            Rentals.Clear();
            var rentalsFromDb = await _service.GetRentalsAsync();

            foreach (var rental in rentalsFromDb)
            {
                Rentals.Add(rental);
            }
        }

        private async Task AddRentalAsync()
        {
            var newRental = new Rental
            {
                CarId = 1,
                ClientId = 1,
                StartDate = System.DateTime.Now,
                EndDate = System.DateTime.Now.AddDays(3),
                DailyRate = 1000
            };

            await _service.AddRentalAsync(newRental);
            await LoadRentalsAsync();
        }
    }
}
