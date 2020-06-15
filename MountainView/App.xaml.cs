using MountainView.Pages;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MountainView
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new SlideShowPage();
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
