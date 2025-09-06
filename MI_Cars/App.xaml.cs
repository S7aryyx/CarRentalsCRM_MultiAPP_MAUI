using Microsoft.Maui.Controls;
using MI_Cars.Views;

namespace MI_Cars
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }
    }
}
