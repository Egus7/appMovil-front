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

namespace consumeAPImmarketdemo.Paginas.ModuloFacturacion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FacClientes : TabbedPage
    {
        APIConsume api = new APIConsume();


        public FacClientes ()
        {
            InitializeComponent();

            // Obtener la lista de clientes
            List<Cliente> clientes = ObtenerClientes();

            // Crear la TableSection
            TableSection clientesSection = new TableSection();

            // Iterar sobre la lista de clientes y crear una ViewCell para cada
            // cliente
            foreach (Cliente cliente in clientes)
            {
                ViewCell viewCell = CrearViewCellCliente(cliente);
                clientesSection.Add(viewCell);
            }

            // Obtener el TableRoot de la TableView
            var tableRoot = ClientsTableView.Root;

            // Agregar la TableSection al TableRoot
            tableRoot.Add(clientesSection);
        }


        private List<Cliente> ObtenerClientes()
        {
            List<Cliente> listClientes = new List<Cliente>();

            try
            {
                using (WebClient wc = new WebClient())
                {

                    wc.Headers.Add("Content-Type", "application/json");

                    var url = $"{api.BaseUrlFac}/clientes";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de productos
                        listClientes = JsonConvert.DeserializeObject<List<Cliente>>(json);
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

            return listClientes;

        }

        private void Button_Insertar(object sender, EventArgs e)
        {

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(txtCedula.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Crea un objeto Cliente con los datos del nuevo Cliente a insertar
            var nuevoCliente = new Models.Cliente
            {
                cedula_cliente = txtCedula.Text,
                nombres = txtNombre.Text,
                apellidos = txtApellido.Text,
                direccion = txtDireccion.Text
            };

            // Convierte el objeto Producto a JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(nuevoCliente);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                try
                {

                    // Realiza la solicitud POST para insertar el nuevo Producto
                    var response = wc.UploadString(api.BaseUrlFac + "/clientes", "POST", json);

                    Application.Current.MainPage.DisplayAlert("Exito", "Cliente insertado", "OK");

                    // Actualizar la tabla de clientes
                    ActualizarTablaClientes();
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error", "Error en la solicitud HTTP: " + ex.Message, "Cerrar");
                }
            }
        }

        private void Button_Actualizar(object sender, EventArgs e)
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(txtCedula.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Crea un objeto Cliente con los datos actualizados Cliente a actualizar
            var clienteActualizado = new Models.Cliente
            {
                cedula_cliente = txtCedula.Text,
                nombres = txtNombre.Text,
                apellidos = txtApellido.Text,
                direccion = txtDireccion.Text
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(clienteActualizado);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                try
                {
                    // Realiza la solicitud PUT para actualizar el Cliente
                    var response = wc.UploadString(api.BaseUrlFac + "/clientes/" + clienteActualizado.cedula_cliente, "PUT", json);

                    // Muestra un diálogo con el mensaje de éxito
                    Application.Current.MainPage.DisplayAlert("Éxito", "Cliente actualizado correctamente", "OK");

                    // Actualizar la tabla de clientes
                    ActualizarTablaClientes();
                }
                catch (WebException ex)
                {
                    // Maneja el error de la solicitud HTTP
                    Application.Current.MainPage.DisplayAlert("Error", "Error en la solicitud HTTP: " + ex.Message, "Cerrar");
                }
            }
        }

        private void Button_Limpiar(object sender, EventArgs e)
        {
            //Limpia los campos de texto
            txtCedula.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDireccion.Text = "";

        }

        private async Task EliminarCliente(Cliente cliente)
        {
            bool confirmarEliminar = await Application.Current.MainPage.DisplayAlert("Eliminar cliente", "¿Está seguro de eliminar este cliente?", "Sí", "No");

            if (confirmarEliminar)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("Content-Type", "application/json");

                        // Realiza la solicitud DELETE para eliminar el usuario
                        wc.UploadString(api.BaseUrlFac + "/clientes/" + cliente.cedula_cliente, "DELETE", "");

                        // Muestra un mensaje de éxito
                        await Application.Current.MainPage.DisplayAlert("Éxito", "Cliente eliminado", "OK");

                        // Actualizar la tabla de clientes
                        ActualizarTablaClientes();
                    }
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error que pueda ocurrir durante la eliminación del usuario
                    await Application.Current.MainPage.DisplayAlert("Error", "Error al eliminar el producto: " + ex.Message, "Cerrar");
                }
            }
        }

        //se actualiza la tabla luego de hacer algun crud (post, put o delete)
        private void ActualizarTablaClientes()
        {
            // Obtener la lista de clientes desde alguna fuente de datos
            List<Cliente> clientes = ObtenerClientes();

            // Obtener el TableRoot de la TableView
            var tableRoot = ClientsTableView.Root;

            // Crear la TableSection
            TableSection clientesSection = new TableSection();

            // Obtener la ViewCell de la cabecera por su nombre
            var cabeceraViewCell = cabecera;
            // Agregar la cabecera al TableSection
            clientesSection.Add(cabeceraViewCell);

            // Iterar sobre la lista de clientes y crear una ViewCell
            // para cada cliente
            foreach (Cliente cliente in clientes)
            {
                // Crear la ViewCell con los datos del usuario
                ViewCell viewCell = CrearViewCellCliente(cliente);
                clientesSection.Add(viewCell);
            }

            // Limpiar el TableRoot
            tableRoot.Clear();

            // Agregar la TableSection actualizada al TableRoot
            tableRoot.Add(clientesSection);
        }

        private ViewCell CrearViewCellCliente(Cliente cliente)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label codigoLabel = new Label { Text = cliente.cedula_cliente, HorizontalOptions = LayoutOptions.Center };
            Label nombreLabel = new Label { Text = cliente.nombres, HorizontalOptions = LayoutOptions.Center };
            Label descripcionLabel = new Label { Text = cliente.apellidos, HorizontalOptions = LayoutOptions.Center };
            Label precioLabel = new Label { Text = cliente.direccion, HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(codigoLabel, 0, 0);
            grid.Children.Add(nombreLabel, 1, 0);
            grid.Children.Add(descripcionLabel, 2, 0);
            grid.Children.Add(precioLabel, 3, 0);

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
                string cedula = cliente.cedula_cliente;
                string nombres = cliente.nombres;
                string apellidos = cliente.apellidos;
                string direccion = cliente.direccion;

                // Establecer los valores en la página de edición
                if (crudClientes != null)
                {
                    ((Entry)crudClientes.FindByName("txtCedula")).Text = cedula;
                    ((Entry)crudClientes.FindByName("txtNombre")).Text = nombres;
                    ((Entry)crudClientes.FindByName("txtApellido")).Text = apellidos;
                    ((Entry)crudClientes.FindByName("txtDireccion")).Text = direccion;

                    // Cambiar a la página de edición
                    CurrentPage = crudClientes;
                }
            };

            eliminarButton.Clicked += async (sender, e) =>
            {
                // Acción para eliminar el cliente
                await EliminarCliente(cliente);
            };

            // Crear el StackLayout y agregar los botones
            StackLayout opcionesLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Children = { editarButton, eliminarButton }
            };

            grid.Children.Add(opcionesLayout, 4, 0);

            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }


    }
}