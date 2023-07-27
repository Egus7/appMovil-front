using consumeAPI_mmarketdemo;
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
    public partial class Registro : ContentPage
    {

        public Registro()
        {
            InitializeComponent();
        }

        APIConsume URL = new APIConsume();
        private string endURL = "/apirest/seguridades";

        private Models.SegUsuario[] Usuarios { get; set; }

        private void Button_Reg(object sender, EventArgs e)
        {
            // Crea un objeto SegUsuario con los datos del nuevo usuario a insertar
            var nuevoUsuario = new Models.SegUsuario
            {
                codigo = txtCodigo.Text,
                apellidos = txtApellidos.Text,
                nombres = txtNombres.Text,
                correo = txtCorreo.Text,
                clave = txtClave.Text,
                activo = true
            };

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                string.IsNullOrWhiteSpace(txtCorreo.Text) ||
                string.IsNullOrWhiteSpace(txtClave.Text))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Convierte el objeto SegUsuario a JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(nuevoUsuario);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                try
                {
                    bool codigoExiste = VerificarExistencia("codigo", nuevoUsuario.codigo);
                    if (codigoExiste)
                    {
                        Application.Current.MainPage.DisplayAlert("Error", "El usuario ya existe", "Cerrar");
                        return;
                    }
                    bool correoExiste = VerificarExistencia("correo", nuevoUsuario.correo);
                    if (correoExiste)
                    {
                        Application.Current.MainPage.DisplayAlert("Error", "El correo ya está registrado", "Cerrar");
                        return;
                    }
                    // Realiza la solicitud POST para insertar el nuevo usuario
                    var response = wc.UploadString(URL.BaseUrl + endURL + "/registro", "POST", json);
                    Application.Current.MainPage.DisplayAlert("Exito", "Usuario registrado correctamente", "OK");

                    App.Current.MainPage = new Login();            
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error","Error al registrar usuario: " + ex.Message, "Cerrar");
                } 
            }
        }

        private void Button_Cancelar(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Login());
        }

        private bool VerificarExistencia(string campo, string valor)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    string url = $"{URL.BaseUrl}" + endURL + $"/usuarios/{campo}/{valor}";
                    var response = wc.DownloadString(url);
                    dynamic responseObject = JsonConvert.DeserializeObject(response);
                    bool existe = responseObject.existe;
                    return existe;
                }
            }
            catch (WebException ex)
            {
                // Manejar error de conexión o solicitud HTTP
                Application.Current.MainPage.DisplayAlert("Error", "Error al registrar usuario: " + ex.Message, "Cerrar");
                return false;
            }
        }

        private void SwitchMostrarClave_Toggled(object sender, ToggledEventArgs e)
        {
            txtClave.IsPassword = !e.Value;
        }

    }
}