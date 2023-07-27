using System;
using Xamarin.Forms;
using Rg.Plugins.Popup;
using Xamarin.Forms.Xaml;

namespace consumeAPI_mmarketdemo
{
    public partial class App : Application
    {
        public App()
        {

            MainPage = new NavigationPage(new consumeAPImmarketdemo.Paginas.Login());

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
