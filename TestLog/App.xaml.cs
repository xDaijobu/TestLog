using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TestLog
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new NavigationPage(new PageA());
            //MainPage = new MainPage();
            MainPage = new NavigationPage(new MainPage());
        }

        public class PageA : ContentPage
        {
            public PageA()
            {

            }

            protected override void OnAppearing()
            {
                base.OnAppearing();

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var mainPage = new MainPage();
                        await Task.Delay(1000);
                        await Navigation.PushModalAsync(mainPage);
                    });
                });
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
