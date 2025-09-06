using Microsoft.Maui.Controls;
using MI_Cars.ViewModels;
using System;

namespace MI_Cars.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _vm;

        public MainPage()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitializeAsync();
        }

        private async void OnGoToCarsClicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushAsync(new CarsPage());
        }

        private async void OnGoToRentalsClicked(object sender, EventArgs e)
        {
            await App.Current.MainPage.Navigation.PushAsync(new RentalsPage());
        }
    }
}
