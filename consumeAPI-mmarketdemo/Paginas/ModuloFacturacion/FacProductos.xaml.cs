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
    public partial class FacProductos : TabbedPage
    {
        APIConsume api = new APIConsume();

        public FacProductos ()
        {
            InitializeComponent();

            // Obtener la lista de productos 
            List<Producto> productos = ObtenerProductos();

            // Crear la TableSection
            TableSection productosSection = new TableSection();

            // Iterar sobre la lista de productos y crear una ViewCell para cada producto
            foreach (Producto producto in productos)
            {
                ViewCell viewCell = CrearViewCellProducto(producto);
                productosSection.Add(viewCell);

            }

            // Obtener el TableRoot de la TableView
            var tableRoot = ProdsTableView.Root;

            // Agregar la TableSection al TableRoot
            tableRoot.Add(productosSection);
        }

        private List<Producto> ObtenerProductos()
        {
            List<Producto> listProductos = new List<Producto>();

            try
            {
                using (WebClient wc = new WebClient())
                {
                   
                    wc.Headers.Add("Content-Type", "application/json");

                    var url = $"{api.BaseUrlFac}/productos";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de productos
                        listProductos = JsonConvert.DeserializeObject<List<Producto>>(json);
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

            return listProductos;

        }

        private void Button_Insertar(object sender, EventArgs e)
        {

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) ||
                string.IsNullOrWhiteSpace(txtImpuesto.Text))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Crea un objeto Producto con los datos del nuevo Producto a insertar
            var nuevoProducto = new Models.Producto
            {
                codigo_producto = Convert.ToInt32(txtCodigo.Text),
                nombre = txtNombre.Text,
                descripcion = txtDescripcion.Text,
                precio_unitario = Convert.ToDouble(txtPrecio.Text),
                existencia = 0,
                tiene_impuesto = txtImpuesto.Text
            };

            // Convierte el objeto Producto a JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(nuevoProducto);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                try
                {

                    // Realiza la solicitud POST para insertar el nuevo Producto
                    var response = wc.UploadString(api.BaseUrlFac + "/productos", "POST", json);

                    Application.Current.MainPage.DisplayAlert("Exito", "Producto insertado", "OK");

                    // Actualizar la tabla de productos
                    ActualizarTablaProductos();
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
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) ||
                string.IsNullOrWhiteSpace(txtImpuesto.Text))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Crea un objeto Producto con los datos actualizados del Producto
            var productoActualizado = new Models.Producto
            {
                codigo_producto = Convert.ToInt32(txtCodigo.Text),
                nombre = txtNombre.Text,
                descripcion = txtDescripcion.Text,
                precio_unitario = Convert.ToDouble(txtPrecio.Text),
                tiene_impuesto = txtImpuesto.Text
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(productoActualizado);

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                try
                {
                    // Realiza la solicitud PUT para actualizar el Producto
                    var response = wc.UploadString(api.BaseUrlFac + "/productos/" + productoActualizado.codigo_producto, "PUT", json);

                    // Muestra un diálogo con el mensaje de éxito
                    Application.Current.MainPage.DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");

                    // Actualizar la tabla de productos
                    ActualizarTablaProductos();
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
            txtCodigo.Text = "";
            txtNombre.Text = "";
            txtDescripcion.Text = "";
            txtPrecio.Text = "";
            txtImpuesto.Text = "";

        }

        private async Task EliminarProducto(Producto producto)
        {
            bool confirmarEliminar = await Application.Current.MainPage.DisplayAlert("Eliminar producto", "¿Está seguro de eliminar este producto?", "Sí", "No");

            if (confirmarEliminar)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("Content-Type", "application/json");

                        // Realiza la solicitud DELETE para eliminar el usuario
                        wc.UploadString(api.BaseUrlFac + "/productos/" + producto.codigo_producto, "DELETE", "");

                        // Muestra un mensaje de éxito
                        await Application.Current.MainPage.DisplayAlert("Éxito", "Producto eliminado", "OK");

                        // Actualizar la tabla de productos
                        ActualizarTablaProductos();
                    }
                }
                catch (Exception ex)
                {
                    // Maneja cualquier error que pueda ocurrir durante la eliminación del usuario
                    await Application.Current.MainPage.DisplayAlert("Error", "Error al eliminar el producto: " + ex.Message, "Cerrar");
                }
            }
        }

        //se actuliza la tabla luego de hacer algun crud (post, put o delete)
        private void ActualizarTablaProductos()
        {
            // Obtener la lista de usuarios desde alguna fuente de datos
            List<Producto> productos = ObtenerProductos();

            // Obtener el TableRoot de la TableView
            var tableRoot = ProdsTableView.Root;

            // Crear la TableSection
            TableSection productosSection = new TableSection();

            // Obtener la ViewCell de la cabecera por su nombre
            var cabeceraViewCell = cabecera;
            // Agregar la cabecera al TableSection
            productosSection.Add(cabeceraViewCell);

            // Iterar sobre la lista de productos y crear una ViewCell para cada productos
            foreach (Producto producto in productos)
            {
                // Crear la ViewCell con los datos del usuario
                ViewCell viewCell = CrearViewCellProducto(producto);
                productosSection.Add(viewCell);
            }

            // Limpiar el TableRoot
            tableRoot.Clear();

            // Agregar la TableSection actualizada al TableRoot
            tableRoot.Add(productosSection);
        }

        private ViewCell CrearViewCellProducto(Producto producto)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label codigoLabel = new Label { Text = producto.codigo_producto.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label nombreLabel = new Label { Text = producto.nombre, HorizontalOptions = LayoutOptions.Center };
            Label descripcionLabel = new Label { Text = producto.descripcion, HorizontalOptions = LayoutOptions.Center };
            Label precioLabel = new Label { Text = producto.precio_unitario.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label stockLabel = new Label { Text = producto.existencia.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label ivaLabel = new Label { Text = producto.tiene_impuesto, HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(codigoLabel, 0, 0);
            grid.Children.Add(nombreLabel, 1, 0);
            grid.Children.Add(descripcionLabel, 2, 0);
            grid.Children.Add(precioLabel, 3, 0);
            grid.Children.Add(stockLabel, 4, 0);
            grid.Children.Add(ivaLabel, 5, 0);

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
                int codigo = producto.codigo_producto;
                string nombre = producto.nombre;
                string descripcion = producto.descripcion;
                double precio = producto.precio_unitario;
                string iva = producto.tiene_impuesto;

                // Establecer los valores en la página de edición
                if (crudProductos != null)
                {
                    ((Entry)crudProductos.FindByName("txtCodigo")).Text = codigo.ToString();
                    ((Entry)crudProductos.FindByName("txtNombre")).Text = nombre;
                    ((Entry)crudProductos.FindByName("txtDescripcion")).Text = descripcion;
                    ((Entry)crudProductos.FindByName("txtPrecio")).Text = precio.ToString();
                    ((Entry)crudProductos.FindByName("txtImpuesto")).Text = iva;

                    // Cambiar a la página de edición
                    CurrentPage = crudProductos;
                }
            };

            eliminarButton.Clicked += async (sender, e) =>
            {
                // Acción para eliminar el usuario
                await EliminarProducto(producto);
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