using Market.Views;
using Market.Services;
using Market.ViewModels;

namespace Market
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}