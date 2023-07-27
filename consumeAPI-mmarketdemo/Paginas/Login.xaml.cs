using consumeAPImmarketdemo.API;
using consumeAPImmarketdemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace consumeAPImmarketdemo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {

        APIConsume api = new APIConsume();

        public Login()
        {
            InitializeComponent();
        }

        private async void Button_Reg_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registro());
        }

        private async void Button_Login_Clicked(object sender, EventArgs e)
        {
            string codigo = EntryCodigo.Text;
            string clave = EntryClave.Text;

            //devuelve el token una vez logeado
            string token = await AutenticarUsuario(codigo, clave);

            //SegUsuario usuario = await ObtenerUsuario(codigo, clave);

            if (!string.IsNullOrEmpty(token))
            {
                
                await Navigation.PushAsync(new Modulos(token));
            }
            else
            {
                // La autenticación falló, mostrar mensaje de error
                await DisplayAlert("Error", "Inicio de sesión fallido", "Cerrar");

            }
        }

    private async Task<string> AutenticarUsuario(string codigo, string clave)
        {
            // Crear un objeto JSON con las credenciales
            var json = JsonConvert.SerializeObject(new { codigo, clave });

            using (var client = new HttpClient())
            {                
                var url = $"{api.BaseUrl}/login";
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var token = data.token;

                    return token;
                }
                else
                {
                    // La autenticación falló, manejar el error de acuerdo a tus necesidades
                    return null;
                }
            }
        }

        private void SwitchMostrarClave_Toggled(object sender, ToggledEventArgs e)
        {
            EntryClave.IsPassword = !e.Value;
        }


    }
}