using consumeAPImmarketdemo.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using consumeAPImmarketdemo.API;

namespace consumeAPImmarketdemo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Modulos : ContentPage
    {

        private string Token { get; }
        

        public Modulos(string token)
        {
            InitializeComponent();
            Token = token;

            CargarModulosAsignados();
        }

        private async void CargarModulosAsignados()
        {
            // Obtener el ID del usuario a partir del token
            int idUsuario = ObtenerIdUsuario(Token);

            // Obtener los módulos asignados al usuario
            List<SegModulo> modulosAsignados = await ObtenerModulosAsignados(idUsuario);

            if (modulosAsignados != null && modulosAsignados.Count > 0)
            {
                bool tieneModulosActivos = false;

                // Mostrar los módulos asignados en la interfaz de usuario
                foreach (SegModulo modulo in modulosAsignados)
                {
                    if (modulo.id_seg_modulo == 1 || modulo.id_seg_modulo == 9)
                    {
                        Button botonModulo = new Button
                        {
                            Text = modulo.nombre_modulo,
                            Command = new Command(() => AbrirModulo(modulo.id_seg_modulo))
                        };
                        stackLayout.Children.Add(botonModulo);

                        tieneModulosActivos = true;
                    }
                }

                if (!tieneModulosActivos)
                {
                    // Mostrar un mensaje si el usuario no tiene módulos asignados activos (1 o 9)
                    await DisplayAlert("Mensaje", "Sus modulos asignados estan desactivados", "Cerrar");
                }
            }
            else
            {
                // Mostrar un mensaje si el usuario no tiene módulos asignados
                await DisplayAlert("Sin módulos asignados", "No tiene módulos asignados", "Cerrar");
            }
        }


        private void AbrirModulo(int idModulo)
        {
            // Lógica para abrir el módulo correspondiente
            switch (idModulo)
            {
                case 1:
                    //modulo seguridades
                    Navigation.PushAsync(new ModSeguridades(Token));
                    break;
                case 9:
                    //modulo facturacion
                    Navigation.PushAsync(new ModFacturacion());
                    break;
                // Agrega más casos según tus módulos disponibles
                default:
                    // Módulo no válido, mostrar un mensaje de error o realizar otra acción deseada
                    break;
            }
        }

        private async Task<List<SegModulo>> ObtenerModulosAsignados(int idUsuario)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Access-Token", Token);

                    var api = new APIConsume();
                    string url = $"{api.BaseUrl}/apirest/seguridades/asignaciones/usuarios/{idUsuario}";
                    string response = await wc.DownloadStringTaskAsync(url);
                    List<SegModulo> modulosAsignados = JsonConvert.DeserializeObject<List<SegModulo>>(response);
                    return modulosAsignados;
                }
            }
            catch (WebException ex)
            {
                // Manejar el error de conexión o solicitud HTTP
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private int ObtenerIdUsuario(string token)
        {
            // Decodificar el token y obtener el ID de usuario
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            var idUsuario = Convert.ToInt32(securityToken.Claims.FirstOrDefault(claim => claim.Type == "id_seg_usuario")?.Value);

            return idUsuario;
        }

    }
}