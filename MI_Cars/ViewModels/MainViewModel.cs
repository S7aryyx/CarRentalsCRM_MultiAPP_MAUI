using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MI_Cars.Models;
using MI_Cars.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MI_Cars.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly PostgresService _service = new();

        private bool _isInitialized = false;

        [ObservableProperty] private ObservableCollection<Car> cars = new();
        [ObservableProperty] private ObservableCollection<Car> filteredCars = new();

        [ObservableProperty] private ObservableCollection<string> months;
        [ObservableProperty] private string selectedMonth;

        [ObservableProperty] private ObservableCollection<int> years;
        [ObservableProperty] private int selectedYear;

        [ObservableProperty] private ObservableCollection<string> availableBrandsWithAll = new();
        [ObservableProperty] private string selectedBrand = "Все";

        [ObservableProperty] private string searchPlateNumber;

        public IAsyncRelayCommand<CarDate> DateClickedCommand { get; }

        public MainViewModel()
        {
            Months = new ObservableCollection<string>(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12));
            SelectedMonth = DateTime.Now.ToString("MMMM", CultureInfo.CurrentCulture);

            int currentYear = DateTime.Now.Year;
            Years = new ObservableCollection<int>(Enumerable.Range(currentYear - 5, 11));
            SelectedYear = currentYear;

            DateClickedCommand = new AsyncRelayCommand<CarDate>(DateClicked);
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            await LoadDataAsync();

            // Авто-фильтр при старте: текущий месяц и "Все" марки
            SelectedMonth = DateTime.Now.ToString("MMMM", CultureInfo.CurrentCulture);
            SelectedBrand = "Все";
            ApplyFilters();
        }

        private async Task LoadDataAsync()
        {
            Cars.Clear();
            FilteredCars.Clear();
            AvailableBrandsWithAll.Clear();

            var carsFromDb = await _service.GetCarsAsync();
            var rentalsFromDb = await _service.GetRentalsAsync();

            // Подгружаем марки и добавляем "Все"
            var brands = carsFromDb.Select(c => c.Brand).Distinct().OrderBy(b => b).ToList();
            AvailableBrandsWithAll.Add("Все");
            foreach (var b in brands) AvailableBrandsWithAll.Add(b);

            int monthIndex = DateTime.ParseExact(SelectedMonth, "MMMM", CultureInfo.CurrentCulture).Month;
            int daysInMonth = DateTime.DaysInMonth(SelectedYear, monthIndex);

            foreach (var car in carsFromDb)
            {
                var carRentals = rentalsFromDb.Where(r => r.CarId == car.CarId).ToList();
                car.RentalsForCar = new ObservableCollection<Rental>(carRentals);
                car.DatesForCar.Clear();

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(SelectedYear, monthIndex, day);
                    var rental = carRentals.FirstOrDefault(r =>
                        currentDate.Date >= r.StartDate.Date && currentDate.Date <= r.EndDate.Date);

                    string status = rental == null
                        ? "Free"
                        : rental.StartDate.Date > DateTime.Now.Date
                            ? "Booked"
                            : "Rented";

                    car.DatesForCar.Add(new CarDate
                    {
                        Date = currentDate,
                        Status = status,
                        Rental = rental
                    });
                }

                Cars.Add(car);
            }

            ApplyFilters();
        }

        partial void OnSelectedMonthChanged(string value)
        {
            if (_isInitialized)
                _ = LoadDataAsync();
        }

        partial void OnSelectedYearChanged(int value)
        {
            if (_isInitialized)
                _ = LoadDataAsync();
        }

        partial void OnSelectedBrandChanged(string value) => ApplyFilters();
        partial void OnSearchPlateNumberChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            var filtered = Cars.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchPlateNumber))
            {
                filtered = filtered.Where(c => !string.IsNullOrEmpty(c.PlateNumber) &&
                                               c.PlateNumber.Contains(SearchPlateNumber, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(SelectedBrand) && SelectedBrand != "Все")
            {
                filtered = filtered.Where(c => c.Brand == SelectedBrand);
            }

            FilteredCars = new ObservableCollection<Car>(filtered);
        }

        private async Task DateClicked(CarDate info)
        {
            if (info == null) return;

            string message;

            if (info.Rental == null)
            {
                message = $"Дата: {info.Date:dd.MM.yyyy}\nАвто свободно";
            }
            else
            {
                var rental = info.Rental;
                var client = rental.Client;
                string clientInfo = client != null ? $"{client.FullName}\nТел: {client.Phone}" : "Клиент: -";
                string period = $"{rental.StartDate:dd.MM.yyyy HH:mm} — {rental.EndDate:dd.MM.yyyy HH:mm}";
                string price = $"Цена/сутки: {rental.DailyRate}";
                message = $"{clientInfo}\nПериод: {period}\n{price}";
            }

            await Application.Current.MainPage.DisplayAlert($"Аренда {info.Date:dd.MM.yyyy}", message, "OK");
        }
    }
}
