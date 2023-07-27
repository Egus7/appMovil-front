using consumeAPImmarketdemo.API;
using consumeAPImmarketdemo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace consumeAPImmarketdemo.Paginas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ModAsignaciones : ContentPage
	{

        public string Token { get; }
        public List<SegUsuario> listaUsuarios { get; set; }
        public List<SegPerfil> listaPerfiles { get; set; }

        private List<SegAsignacion> perfilesAsignados;


        public ModAsignaciones (string token)
		{
			InitializeComponent ();
			Token = token;

            // Inicializa y llena la lista de usuarios
            listaUsuarios = ObtenerUsuarios();
            // Inicializa y llena la lista de perfiles
            listaPerfiles = ObtenerPerfiles();

            // Obtén el nombre del módulo para cada perfil y asígnalo
            // a la propiedad nombre_modulo
            foreach (var perfil in listaPerfiles)
            {
                perfil.nombre_modulo = ObtenerNombreModulo(perfil.id_seg_modulo);
            }

            // Asigna la lista de usuarios al Picker
            cmbUsuario.ItemsSource = listaUsuarios;
            // Asigna la lista de perfiles al Picker
            cmbPerfil.ItemsSource = listaPerfiles;
            // nombre de la propiedad a mostrar en el Picker para el usuario
            cmbUsuario.ItemDisplayBinding = new Binding("Usuario");
            cmbPerfil.ItemDisplayBinding = new Binding("NombrePerfilModulo");

            // Inicializa la lista de perfiles asignados
            perfilesAsignados = new List<SegAsignacion>();
            // Oculta la lista de perfiles asignados
            lstPerfilesAsignados.IsVisible = false;
        }

        APIConsume api = new APIConsume();
        private string endApi = "/apirest/seguridades/asignaciones";

        private void cmbUsuario_SelectedIndexChanged(object sender, EventArgs e)
        {
            var usuarioSeleccionado = cmbUsuario.SelectedItem as SegUsuario;
            if (usuarioSeleccionado != null)
            {
                // Obtén los perfiles asignados al usuario
                perfilesAsignados = ObtenerPerfilesAsignados(usuarioSeleccionado.id_seg_usuario);
                // Actualiza la lista de perfiles asignados
                lstPerfilesAsignados.ItemsSource = perfilesAsignados;
                // Muestra la lista de perfiles asignados
                lstPerfilesAsignados.IsVisible = true; // Mostrar la lista
            }
            else
            {
                lstPerfilesAsignados.IsVisible = false; // Ocultar la lista
            }
        }

        private void Button_Insertar(object sender, EventArgs e)
        {
            // Crea un objeto SegUsuario con los datos del nuevo usuario a insertar
            var nuevaAsignacion = new Models.SegAsignacion
            {
                id_seg_usuario = ((SegUsuario) cmbUsuario.SelectedItem) ?.id_seg_usuario ?? 0,
                id_seg_perfil = ((SegPerfil) cmbPerfil.SelectedItem) ? .id_seg_perfil ?? 0,
            };

            // Validar campos requeridos
            if (cmbUsuario.SelectedItem == null ||
                cmbPerfil.SelectedItem == null  )
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Verificar si el usuario ya tiene asignado el perfil seleccionado
            int idUsuario = nuevaAsignacion.id_seg_usuario;
            int idPerfil = nuevaAsignacion.id_seg_perfil;
            bool perfilAsignado = VerificarPerfilAsignado(idUsuario, idPerfil);

            if (perfilAsignado)
            {
                Application.Current.MainPage.DisplayAlert("Error", "El usuario ya tiene asignado ese perfil/modulo", "Cerrar");
                return;
            }

            // Convierte el objeto SegUsuario a JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(nuevaAsignacion);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                try
                {
                    // Realiza la solicitud POST para insertar el nuevo usuario
                    var response = wc.UploadString(api.BaseUrl + endApi, "POST", json);
                    Application.Current.MainPage.DisplayAlert("Exito", "Asignacion insertada", "OK");

                    // Actualiza la lista de asignaciones
                    var asignaciones = ObtenerPerfilesAsignados(idUsuario);

                    // Actualizar la lista de perfiles asignados al usuario seleccionado
                    var usuarioSeleccionado = (SegUsuario)cmbUsuario.SelectedItem;
                    var perfilesAsignados = ObtenerPerfilesAsignados(usuarioSeleccionado.id_seg_usuario);
                    lstPerfilesAsignados.ItemsSource = perfilesAsignados;

                    // Mostrar la lista de perfiles asignados
                    lstPerfilesAsignados.IsVisible = true;
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error", "Error en la solicitud HTTP: " + ex.Message, "Cerrar");
                }
            }
        }

        private List<SegUsuario> ObtenerUsuarios()
        {
            List<SegUsuario> usuarios;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                var json = wc.DownloadString(api.BaseUrl + "/apirest/seguridades/usuarios");

                usuarios = JsonConvert.DeserializeObject<List<SegUsuario>>(json);

                // Filtrar la lista de usuarios para mostrar solo aquellos que estén activos
                usuarios = usuarios.Where(u => u.activo).ToList();
            }

            return usuarios;

        }

        //eliminar una asignacion
        private void Button_EliminarAsignacion(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            var asignacion = (SegAsignacion)button.CommandParameter;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                try
                {
                    // Realiza la solicitud DELETE para eliminar la asignación de perfil
                    var url = $"{api.BaseUrl}/apirest/seguridades/asignaciones/{asignacion.id_seg_asignacion}";
                    wc.UploadString(url, "DELETE", "");

                    // Actualizar la lista de perfiles asignados al usuario seleccionado
                    var usuarioSeleccionado = (SegUsuario)cmbUsuario.SelectedItem;
                    var perfilesAsignados = ObtenerPerfilesAsignados(usuarioSeleccionado.id_seg_usuario);
                    lstPerfilesAsignados.ItemsSource = perfilesAsignados;

                    Application.Current.MainPage.DisplayAlert("Éxito", "Asignación eliminada", "OK");
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error", "Error en la solicitud HTTP: " + ex.Message, "Cerrar");
                }
            }
        }

        private List<SegPerfil> ObtenerPerfiles()
        {
            List<SegPerfil> perfiles;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                var json = wc.DownloadString(api.BaseUrl + "/apirest/seguridades/perfiles");

                perfiles = JsonConvert.DeserializeObject<List<SegPerfil>>(json);

                // Filtrar la lista de perfiles para mostrar solo aquellos con id_seg_perfil igual a 1 o 11
                perfiles = perfiles.Where(p => 
                            p.id_seg_perfil == 1 || 
                            p.id_seg_perfil == 11)
                    .ToList();

            }
            return perfiles;
        }


        private string ObtenerNombreModulo(int idModulo)
        {

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                var json = wc.DownloadString(api.BaseUrl + $"/apirest/seguridades/modulos/{idModulo}");

                var jArray = JArray.Parse(json);
                var modulo = jArray.FirstOrDefault()?.ToObject<SegModulo>();

                return modulo?.nombre_modulo;
            }          
        }

        // verificar si el usuario ya tiene asignado el perfil seleccionado
        private bool VerificarPerfilAsignado(int idUsuario, int idPerfil)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                var url = $"{api.BaseUrl}/apirest/seguridades/asignaciones/perfiles/{idUsuario}";
                var json = wc.DownloadString(url);
                var asignaciones = JsonConvert.DeserializeObject<List<SegAsignacion>>(json);

                // Verificar si existe una asignación con el mismo id_seg_perfil
                return asignaciones.Any(asignacion => asignacion.id_seg_perfil == idPerfil);
            }
        }

        // lista de perfiles asignados a un usuario mediante su Id
        private List<SegAsignacion> ObtenerPerfilesAsignados(int idUsuario)
        {
            List<SegAsignacion> asignaciones;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                var url = $"{api.BaseUrl}/apirest/seguridades/asignaciones/perfiles/{idUsuario}";
                var json = wc.DownloadString(url);
                asignaciones = JsonConvert.DeserializeObject<List<SegAsignacion>>(json);
            }
            // Consulta adicional para obtener el nombre del perfil
            foreach (var asignacion in asignaciones)
            {
                var perfil = ObtenerNombrePerfil(asignacion.id_seg_perfil);
                asignacion.nombre_perfil = perfil;
            }

            return asignaciones;
        }

        //// para el list.view de las asignaciones a cada usuario
        //private List<SegAsignacion> ObtenerAsignaciones(int idUsuario)
        //{
        //    List<SegAsignacion> asignaciones;
        //    using (var wc = new WebClient())
        //    {
        //        wc.Headers.Add("Content-Type", "application/json");
        //        wc.Headers.Add("Access-Token", Token);

        //        var url = $"{api.BaseUrl}/apirest/seguridades/asignaciones/perfiles/{idUsuario}";
        //        var json = wc.DownloadString(url);
        //        asignaciones = JsonConvert.DeserializeObject<List<SegAsignacion>>(json);
        //    }

        //    foreach (var asignacion in asignaciones)
        //    {
        //        var perfil = ObtenerNombrePerfil(asignacion.id_seg_perfil);
        //        asignacion.nombre_perfil = perfil;
        //    }

        //    return asignaciones;
        //}

        private string ObtenerNombrePerfil(int idPerfil)
        {
            string nombrePerfil = string.Empty;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                var json = wc.DownloadString(api.BaseUrl + $"/apirest/seguridades/perfiles/{idPerfil}");

                var jArray = JArray.Parse(json);
                var perfil = jArray.FirstOrDefault()?.ToObject<SegPerfil>();

                if (perfil != null)
                {
                    nombrePerfil = perfil.nombre_perfil;
                }
            }

            return nombrePerfil;
        }


    }
}