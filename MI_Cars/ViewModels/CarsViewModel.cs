using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MI_Cars.Models;
using MI_Cars.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MI_Cars.ViewModels
{
    public partial class CarsViewModel : ObservableObject
    {
        private readonly PostgresService _service = new PostgresService();

        [ObservableProperty]
        private ObservableCollection<Car> cars;

        [ObservableProperty]
        private string plateToDelete;

        public CarsViewModel()
        {
            Cars = new ObservableCollection<Car>();

            LoadCarsCommand = new AsyncRelayCommand(LoadCarsAsync);
            AddCarCommand = new AsyncRelayCommand(AddCarAsync);
            DeleteCarCommand = new AsyncRelayCommand<string>(DeleteCarAsync);
            DeleteCarByPlateCommand = new AsyncRelayCommand(DeleteCarByPlateAsync);

            _ = LoadCarsAsync();
        }

        public IAsyncRelayCommand LoadCarsCommand { get; }
        public IAsyncRelayCommand AddCarCommand { get; }
        public IAsyncRelayCommand<string> DeleteCarCommand { get; }
        public IAsyncRelayCommand DeleteCarByPlateCommand { get; }

        private async Task LoadCarsAsync()
        {
            Cars.Clear();
            var list = await _service.GetCarsAsync();
            foreach (var car in list)
                Cars.Add(car);
        }

        private async Task AddCarAsync()
        {
            // Ввод марки
            string brand = await Application.Current.MainPage.DisplayPromptAsync(
                "Добавить авто", "Введите марку автомобиля:");
            if (string.IsNullOrWhiteSpace(brand)) return;

            // Ввод модели
            string model = await Application.Current.MainPage.DisplayPromptAsync(
                "Добавить авто", "Введите модель автомобиля:");
            if (string.IsNullOrWhiteSpace(model)) return;

            // Ввод госномера
            string plate = await Application.Current.MainPage.DisplayPromptAsync(
                "Добавить авто", "Введите ГосНомер автомобиля:");
            if (string.IsNullOrWhiteSpace(plate)) return;

            // Ввод цвета
            string color = await Application.Current.MainPage.DisplayPromptAsync(
                "Добавить авто", "Введите цвет автомобиля:");
            if (string.IsNullOrWhiteSpace(color)) return;

            // Создаём новый объект
            var newCar = new Car
            {
                Brand = brand,
                Model = model,
                PlateNumber = plate,
                Color = color
            };

            // Добавляем в базу
            newCar.CarId = await _service.AddCarAsync(newCar);

            // Добавляем в ObservableCollection, обновляя CollectionView
            Cars.Add(newCar);
        }


        private async Task DeleteCarAsync(string plateNumber)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Подтверждение", $"Удалить авто с номером {plateNumber}?", "Да", "Нет");

            if (!confirm) return;

            await _service.DeleteCarAsync(plateNumber);
            var carToRemove = Cars.FirstOrDefault(c => c.PlateNumber == plateNumber);
            if (carToRemove != null)
                Cars.Remove(carToRemove);
        }

        private async Task DeleteCarByPlateAsync()
        {
            if (string.IsNullOrWhiteSpace(PlateToDelete))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите ГосНомер", "Ок");
                return;
            }

            await DeleteCarAsync(PlateToDelete);
            PlateToDelete = string.Empty;
        }
    }
}
