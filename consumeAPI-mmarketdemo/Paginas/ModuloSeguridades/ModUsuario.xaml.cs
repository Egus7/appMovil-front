using consumeAPImmarketdemo.API;
using consumeAPImmarketdemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace consumeAPImmarketdemo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModUsuario : TabbedPage
    {

        public string Token { get; set; }

       //private ContentPage editarUsuarioPage;

        APIConsume api = new APIConsume();
        public string endURL = "/apirest/seguridades/usuarios";

        public ModUsuario (String token)
        {
            InitializeComponent();
            Token = token;

            // Obtener la lista de usuarios desde alguna fuente de datos
            List<SegUsuario> usuarios = ObtenerUsuarios();

            // Crear la TableSection
            TableSection usuariosSection = new TableSection();

            // Iterar sobre la lista de usuarios y crear una ViewCell para cada usuario
            foreach (SegUsuario usuario in usuarios)
            {
                ViewCell viewCell = CrearViewCellUsuario(usuario);
                usuariosSection.Add(viewCell);
            }

            // Obtener el TableRoot de la TableView
            var tableRoot = UsersTableView.Root;

            // Agregar la TableSection al TableRoot
            tableRoot.Add(usuariosSection);

        }

        private List<SegUsuario> ObtenerUsuarios()
        {
            List<SegUsuario> listUsuarios = new List<SegUsuario>();


            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");
                    wc.Headers.Add("Access-Token", Token);

                    var url = $"{api.BaseUrl}/apirest/seguridades/usuarios";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de usuarios
                        listUsuarios = JsonConvert.DeserializeObject<List<SegUsuario>>(json);
                    }
                    catch (JsonException ex)
                    {
                        // Manejar el error de deserialización JSON
                        DisplayAlert("Error", "Error al deserializar la respuesta JSON: " + ex.Message, "Cerrar");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error al obtener los usuarios: " + ex.Message, "Cerrar");
            }

            return listUsuarios;

        }

        private void Button_Insertar(object sender, EventArgs e)
        {
            // Crea un objeto SegUsuario con los datos del nuevo usuario a insertar
            var nuevoUsuario = new Models.SegUsuario
            {
                codigo = txtCodigo.Text,
                apellidos = txtApellidos.Text,
                nombres = txtNombres.Text,
                correo = txtCorreo.Text,
                clave = txtClave.Text,
                activo = swActivo.IsToggled
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
                wc.Headers.Add("Access-Token", Token);

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
                    // Validar formato del correo
                    if (!VerificarFormatoCorreo(txtCorreo.Text))
                    {
                        Application.Current.MainPage.DisplayAlert("Error", "El formato del correo no es válido", "Cerrar");
                        return;
                    }
                    // Realiza la solicitud POST para insertar el nuevo usuario
                    var response = wc.UploadString(api.BaseUrl + endURL, "POST", json);
                    Application.Current.MainPage.DisplayAlert("Exito", "Usuario insertado", "OK");

                    // Actualizar la tabla de usuarios
                    ActualizarTablaUsuarios();
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error", "Error en la solicitud HTTP: " + ex.Message, "Cerrar");
                }
            }
        }

        private bool VerificarFormatoCorreo(string correo)
        {
            // Expresión regular para validar el formato del correo
            string formatoCorreo = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

            // Verificar si el correo cumple con el formato
            return System.Text.RegularExpressions.Regex.IsMatch(correo, formatoCorreo);
        }


        private void SwitchMostrarClave_Toggled(object sender, ToggledEventArgs e)
        {
            txtClave.IsPassword = !e.Value;
        }


        private bool VerificarExistencia(string campo, string valor)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    string url = $"{api.BaseUrl}" + endURL + $"/{campo}/{valor}";
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

        private void Button_Limpiar(object sender, EventArgs e)
        {
            txtId.Text = "";
            txtCodigo.Text = "";
            txtNombres.Text = "";
            txtApellidos.Text = "";
            txtCorreo.Text = "";
            txtClave.Text = "";
            swActivo.IsToggled = false;

        }

        //Actualizar un usuario
        private void Button_Actualizar(object sender, EventArgs e)
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(txtId.Text) ||
                string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                string.IsNullOrWhiteSpace(txtCorreo.Text) ||
                string.IsNullOrWhiteSpace(txtClave.Text))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Crea un objeto SegUsuario con los datos actualizados del usuario
            var usuarioActualizado = new Models.SegUsuario
            {
                id_seg_usuario = Convert.ToInt32(txtId.Text),
                codigo = txtCodigo.Text,
                apellidos = txtApellidos.Text,
                nombres = txtNombres.Text,
                correo = txtCorreo.Text,
                clave = txtClave.Text,
                activo = swActivo.IsToggled
            };

            // Convierte el objeto SegUsuario a JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(usuarioActualizado);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                wc.Headers.Add("Access-Token", Token);

                try
                {
                    // Realiza la solicitud PUT para actualizar el usuario
                    var response = wc.UploadString(api.BaseUrl + endURL + "/" + usuarioActualizado.id_seg_usuario, "PUT", json);

                    // Muestra un diálogo con el mensaje de éxito
                    Application.Current.MainPage.DisplayAlert("Éxito", "Usuario actualizado correctamente", "OK");

                    // Actualizar la tabla de usuarios
                    ActualizarTablaUsuarios();
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error", "Error en la solicitud HTTP: " + ex.Message, "Cerrar");
                }
            }
        }

        private async Task EliminarUsuario(SegUsuario usuario)
        {
            bool confirmarEliminar = await Application.Current.MainPage.DisplayAlert("Eliminar usuario", "¿Está seguro de eliminar este usuario?", "Sí", "No");

            if (confirmarEliminar)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("Content-Type", "application/json");
                        wc.Headers.Add("Access-Token", Token);

                        //llamar a la clase APIConsume
                        APIConsume api = new APIConsume();

                        // Realiza la solicitud DELETE para eliminar el usuario
                        wc.UploadString(api.BaseUrl + endURL + "/" + usuario.id_seg_usuario, "DELETE", "");

                        // Muestra un mensaje de éxito
                        await Application.Current.MainPage.DisplayAlert("Éxito", "Usuario eliminado", "OK");

                        // Actualizar la tabla de usuarios
                        ActualizarTablaUsuarios();
                    }
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error que pueda ocurrir durante la eliminación del usuario
                    await Application.Current.MainPage.DisplayAlert("Error", "Error al eliminar el usuario: " + ex.Message, "Cerrar");
                }
            }
        }

        //se actuliza la tabla luego de eliminar un usuario
        private void ActualizarTablaUsuarios()
        {
            // Obtener la lista de usuarios desde alguna fuente de datos
            List<SegUsuario> usuarios = ObtenerUsuarios();

            // Obtener el TableRoot de la TableView
            var tableRoot = UsersTableView.Root;

            // Crear la TableSection
            TableSection usuariosSection = new TableSection();

            // Obtener la ViewCell de la cabecera por su nombre
            var cabeceraViewCell = cabecera;
            // Agregar la cabecera al TableSection
            usuariosSection.Add(cabeceraViewCell);

            // Iterar sobre la lista de usuarios y crear una ViewCell para cada usuario
            foreach (SegUsuario usuario in usuarios)
            {
                // Crear la ViewCell con los datos del usuario
                ViewCell viewCell = CrearViewCellUsuario(usuario);
                usuariosSection.Add(viewCell);
            }

            // Limpiar el TableRoot
            tableRoot.Clear();

            // Agregar la TableSection actualizada al TableRoot
            tableRoot.Add(usuariosSection);
        }

        private ViewCell CrearViewCellUsuario(SegUsuario usuario)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label idLabel = new Label { Text = usuario.id_seg_usuario.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label codigoLabel = new Label { Text = usuario.codigo, HorizontalOptions = LayoutOptions.Center };
            Label nombresLabel = new Label { Text = usuario.nombres, HorizontalOptions = LayoutOptions.Center };
            Label apellidosLabel = new Label { Text = usuario.apellidos, HorizontalOptions = LayoutOptions.Center };
            Label correoLabel = new Label { Text = usuario.correo, HorizontalOptions = LayoutOptions.Center };
            Label activoLabel = new Label { Text = usuario.activo.ToString(), HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(idLabel, 0, 0);
            grid.Children.Add(codigoLabel, 1, 0);
            grid.Children.Add(nombresLabel, 2, 0);
            grid.Children.Add(apellidosLabel, 3, 0);
            grid.Children.Add(correoLabel, 4, 0);
            grid.Children.Add(activoLabel, 5, 0);

            ImageButton eliminarButton = new ImageButton
            {
                Source = "https://cdn-icons-png.flaticon.com/512/5974/5974771.png",
                BackgroundColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 17,
                HeightRequest = 17
            };

            ImageButton editarButton = new ImageButton
            {
                Source = "https://cdn-icons-png.flaticon.com/512/1160/1160515.png",
                BackgroundColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 17,
                HeightRequest = 17
            };

            editarButton.Clicked += (sender, e) =>
            {
                // Obtener los valores del usuario seleccionado
                int id = usuario.id_seg_usuario;
                string codigo = usuario.codigo;
                string nombres = usuario.nombres;
                string apellidos = usuario.apellidos;
                string correo = usuario.correo;
                string clave = usuario.clave;
                bool activo = usuario.activo;

                // Establecer los valores en la página de edición
                if (editarUsuarioPage != null)
                {
                    ((Entry)editarUsuarioPage.FindByName("txtId")).Text = id.ToString();
                    ((Entry)editarUsuarioPage.FindByName("txtCodigo")).Text = codigo;
                    ((Entry)editarUsuarioPage.FindByName("txtNombres")).Text = nombres;
                    ((Entry)editarUsuarioPage.FindByName("txtApellidos")).Text = apellidos;
                    ((Entry)editarUsuarioPage.FindByName("txtCorreo")).Text = correo;
                    ((Entry)editarUsuarioPage.FindByName("txtClave")).Text = clave;
                    ((Switch)editarUsuarioPage.FindByName("swActivo")).IsToggled = activo;

                    // Cambiar a la página de edición
                    CurrentPage = editarUsuarioPage;
                }
            };

            eliminarButton.Clicked += async (sender, e) =>
            {
                // Acción para eliminar el usuario
                await EliminarUsuario(usuario);
            };

            // Crear el StackLayout y agregar los botones
            StackLayout opcionesLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Children = { editarButton, eliminarButton }
            };

            grid.Children.Add(opcionesLayout, 6, 0);

            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }



    }
}

