using consumeAPI_mmarketdemo;
using consumeAPImmarketdemo.API;
using consumeAPImmarketdemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace consumeAPImmarketdemo.Paginas.ModuloFacturacion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FacPedidos : TabbedPage
    {
        private List<Cliente> listaClientes { get; set; }
        private List<Producto> listaProductos { get; set; }

        private ObservableCollection<PedidoDet> detalles;

        APIConsume api = new APIConsume();

        public FacPedidos()
        {
            InitializeComponent();

            // Inicializa y llena la lista de clientes
            listaClientes = ObtenerClientes();
            // Inicializa y llena la lista de productos
            listaProductos = ObtenerProductos();
            // Inicializa y llena la lista pedidoscab
            List<PedidoCab> listaCabeceras = ObtenerPedCabeceras();

            // Crea una nueva instancia de ObservableCollection para la lista de detalles
            detalles = new ObservableCollection<PedidoDet>();
            // Establece la lista de detalles como el ItemSource del ListView
            listViewDetalles.ItemsSource = detalles;

            // Asigna la lista de usuarios al Picker
            pickerCliente.ItemsSource = listaClientes;
            // Asigna la lista de productos Picker
            pickerProducto.ItemsSource = listaProductos; 

            // nombre de la propiedad a mostrar en el Picker para el cliente
            pickerCliente.ItemDisplayBinding = new Binding("clienteSel");
            // nombre de la propiedad a mostrar en el Picker para el producto
            pickerProducto.ItemDisplayBinding = new Binding("nombre");

            // Crear la TableSection
            TableSection pedidoscabSection = new TableSection();

            // Iterar sobre la lista de pedidoscab y crear una ViewCell para cada
            // pedidocab
            foreach (PedidoCab pedidoscab in listaCabeceras)
            {
                ViewCell viewCell = CrearViewCellCabeceras(pedidoscab);
                pedidoscabSection.Add(viewCell);
            }

            // Obtener el TableRoot de la TableView
            var tableRoot = PedidosCabTableView.Root;

            // Agregar la TableSection al TableRoot
            tableRoot.Add(pedidoscabSection);


            //// Asignar la lista de detalles del pedido a la TableView
            //List<PedidoDet> listadetalles = ObtenerPedidoDet();
            //// Crear la TableSection
            //TableSection pedidodetsSection = new TableSection();

            //// Iterar sobre la lista de pedidoscab y crear una ViewCell para cada
            //// pedidodets
            //foreach (PedidoDet pedidodets in listadetalles)
            //{
            //    ViewCell viewCell = CrearViewCellDetalles(pedidodets);
            //    pedidodetsSection.Add(viewCell);
            //}

            //// Obtener el TableRoot de la TableView
            //var tableRootDet = PedidosDetTableView.Root;

            //// Agregar la TableSection al TableRoot
            //tableRootDet.Add(pedidodetsSection);
        }

        private List<PedidoCab> ObtenerPedCabeceras()
        {
            List<PedidoCab> listCabeceras = new List<PedidoCab>();

            try
            {
                using (WebClient wc = new WebClient())
                {

                    wc.Headers.Add("Content-Type", "application/json");

                    var url = $"{api.BaseUrlFac}/pedidos";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de pedidos
                        listCabeceras = JsonConvert.DeserializeObject<List<PedidoCab>>(json);
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
                DisplayAlert("Error", "Error al obtener los pedidos: " + ex.Message, "Cerrar");
            }

            return listCabeceras;

        }

        private void OnAgregarDetalleClicked(object sender, EventArgs e)
        {
            // Validar que se haya seleccionado un producto y se haya ingresado la cantidad
            if ((pickerProducto.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(entryCantidad.Text))
            {
                DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Obtener el producto seleccionado en el Picker
            var productoSeleccionado = (Producto)pickerProducto.SelectedItem;

            // Verificar si el producto ya existe en la lista de detalles
            var detalleExistente = detalles.FirstOrDefault(d =>
                    d.codigo_producto == productoSeleccionado.codigo_producto);

            // Convertir la cantidad ingresada a entero
            int cantidadIngresada = Convert.ToInt32(entryCantidad.Text);

            if (detalleExistente != null)
            {
                // Si el producto ya existe, aumentar la cantidad y recalcular el subtotal
                int cantidadTotal = detalleExistente.cantidad + cantidadIngresada;

                detalleExistente.cantidad = cantidadTotal;
                detalleExistente.RecalcularSubtotal();

                // Notificar a la ListView que los datos han cambiado
                listViewDetalles.ItemsSource = null;
                listViewDetalles.ItemsSource = detalles;
            }
            else
            {
                // Si el producto no existe, crear un nuevo objeto PedidoDet
                // con los datos ingresados por el usuario
                var nuevoDetalle = new Models.PedidoDet
                {
                    codigo_producto = productoSeleccionado.codigo_producto,
                    nombre_producto = productoSeleccionado.nombre,
                    precio_unitario_venta = productoSeleccionado.precio_unitario,
                    cantidad = cantidadIngresada,
                    stock = productoSeleccionado.existencia
                };

                // Calcular el subtotal del nuevo detalle
                nuevoDetalle.RecalcularSubtotal();

                // Agregar el nuevo detalle a la lista
                detalles.Add(nuevoDetalle);

                // Notificar a la ListView que se ha agregado un nuevo detalle
                listViewDetalles.ItemsSource = null;
                listViewDetalles.ItemsSource = detalles;
            }

            // Limpiar los campos de entrada después de agregar el detalle
            pickerProducto.SelectedItem = null;
            entryCantidad.Text = string.Empty;
        }


        private List<Cliente> ObtenerClientes()
        {
            List<Cliente> clientes;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                var json = wc.DownloadString(api.BaseUrlFac + "/clientes");

                clientes = JsonConvert.DeserializeObject<List<Cliente>>(json);

            }

            return clientes;

        }

        private List<Producto> ObtenerProductos()
        {
            List<Producto> productos;

            using (var wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");

                var json = wc.DownloadString(api.BaseUrlFac + "/productos");

                productos = JsonConvert.DeserializeObject<List<Producto>>(json);


            }

            return productos;

        }

        public Producto ObtenerProductoCompleto(int idProducto)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var response = wc.DownloadString($"{api.BaseUrlFac}/productos/{idProducto}");
                    return JsonConvert.DeserializeObject<Producto>(response);
                }
            }
            catch (WebException ex)
            {
                // Maneja cualquier error que pueda ocurrir durante la eliminación del usuario
                Application.Current.MainPage.DisplayAlert("Error", "Error al obtener producto: " + ex.Message, "Cerrar");
                return null;
            }
        }

        private void OnEliminarDetalleClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.CommandParameter 
                    is PedidoDet detalleAEliminar)
            {
                detalles.Remove(detalleAEliminar);

                // Notificar a la ListView que los datos han cambiado
                listViewDetalles.ItemsSource = null;
                listViewDetalles.ItemsSource = detalles;
            }
        }

        //se actualiza la tabla luego de hacer algun crud (post)
        private void ActualizarTablaCab()
        {
            // Obtener la lista de pedidoscab desde alguna fuente de datos
            List<PedidoCab> cabeceras = ObtenerPedCabeceras();

            // Obtener el TableRoot de la TableView
            var tableRoot = PedidosCabTableView.Root;

            // Crear la TableSection
            TableSection cabecerasSection = new TableSection();

            // Obtener la ViewCell de la cabecera por su nombre
            var cabeceraViewCell = cabeceraCab;
            // Agregar la cabecera al TableSection
            cabecerasSection.Add(cabeceraViewCell);

            // Iterar sobre la lista de cabeceras y crear una ViewCell
            // para cada cabecera
            foreach (PedidoCab cabecera in cabeceras)
            {
                // Crear la ViewCell con los datos del usuario
                ViewCell viewCell = CrearViewCellCabeceras(cabecera);
                cabecerasSection.Add(viewCell);
            }

            // Limpiar el TableRoot
            tableRoot.Clear();

            // Agregar la TableSection actualizada al TableRoot
            tableRoot.Add(cabecerasSection);
        }

        private void Button_RegistrarPedidoClicked(object sender, EventArgs e)
        {
            // Validar que existan detalles para registrar el pedido
            if (detalles.Count == 0)
            {
                DisplayAlert("Error", "Debe agregar al menos un detalle al pedido", "Cerrar");
                return;
            }

            // Obtener la fecha del DatePicker
            DateTime fechaPedido = fechaPicker.Date;

            // Obtener el cliente seleccionado en el Picker
            Cliente clienteSeleccionado = (Cliente)pickerCliente.SelectedItem;

            // Crear una lista para almacenar los detalles del pedido
            List<PedidoDet> listaDetalles = detalles.ToList();

            // Crear el objeto para enviar al servidor
            var pedido = new
            {
                fecha_pedido = fechaPedido,
                cedula_cliente = clienteSeleccionado.cedula_cliente,
                id_estado_pedido = "NV", // Puedes cambiar esto según el estado deseado
                subtotal = listaDetalles.Sum(detalle => detalle.subtotal),
                detalles = listaDetalles
            };

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");

                    // Convertir el objeto pedido a formato JSON
                    var pedidoJson = JsonConvert.SerializeObject(pedido);

                    // Establecer la URL del endpoint para registrar el pedido
                    string url = $"{api.BaseUrlFac}/registrarpedido";

                    // Realizar la petición POST al servidor
                    var response = wc.UploadString(url, "POST", pedidoJson);

                    // Deserializar la respuesta JSON
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(response);

                    // Verificar si el pedido se registró correctamente
                    if (responseObject.message == "Pedido registrado correctamente")
                    {
                        DisplayAlert("Éxito", "Pedido registrado correctamente", "Cerrar");
                        // Limpiar la lista de detalles
                        detalles.Clear();
                        // Actualizar la ListView para reflejar los cambios
                        listViewDetalles.ItemsSource = null;
                        listViewDetalles.ItemsSource = detalles;

                        // Actualizar la tabla de cabeceras
                        ActualizarTablaCab();
                    }
                    else
                    {
                        DisplayAlert("Error", "No se pudo registrar el pedido", "Cerrar");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Ocurrió un error al registrar el pedido: " + ex.Message, "Cerrar");
            }
        }

        // Método para obtener los detalles del pedido
        private List<PedidoDet> ObtenerPedidoDet(int idPedido)
        {
            List<PedidoDet> listDetalles = new List<PedidoDet>();

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");
                    var url = $"{api.BaseUrlFac}/pedidoscab-detalles/{idPedido}";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de detalles del pedido
                        listDetalles = JsonConvert.DeserializeObject<List<PedidoDet>>(json);
                    }
                    catch (JsonException ex)
                    {
                        // Manejar el error de deserialización JSON
                        Application.Current.MainPage.DisplayAlert("Error", "Error al deserializar la respuesta JSON: " + ex.Message, "Cerrar");
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error al obtener los detalles del pedido: " + ex.Message, "Cerrar");
            }

            return listDetalles;
        }


        private ViewCell CrearViewCellCabeceras(PedidoCab cabecera)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label numeroLabel = new Label { Text = cabecera.numero_pedido.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label fechaLabel = new Label { Text = cabecera.fecha_pedido.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label cedulaLabel = new Label { Text = cabecera.cedula_cliente, HorizontalOptions = LayoutOptions.Center };
            Label estadopedidoLabel = new Label { Text = cabecera.id_estado_pedido, HorizontalOptions = LayoutOptions.Center };
            Label subtotalLabel = new Label { Text = cabecera.subtotal.ToString(), HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(numeroLabel, 0, 0);
            grid.Children.Add(fechaLabel, 1, 0);
            grid.Children.Add(cedulaLabel, 2, 0);
            grid.Children.Add(estadopedidoLabel, 3, 0);
            grid.Children.Add(subtotalLabel, 4, 0);

            ImageButton detalleButton = new ImageButton
            {
                Source = "https://cdn-icons-png.flaticon.com/512/1/1755.png",
                BackgroundColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 17,
                HeightRequest = 17
            };

            detalleButton.Clicked += async (sender, e) =>
            {
                // Obtener la página actual (TabbedPage)
                var tabbedPage = (TabbedPage)Application.Current.MainPage;

                // Navegar a la página de detalles (detallePage) dentro del TabbedPage
                // El segundo parámetro true indica que se debe hacer una animación de navegación
                await tabbedPage.CurrentPage.Navigation.PushAsync(detallePage, true);
            };

            // Crear el StackLayout y agregar los botones
            StackLayout opcionesLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Children = { detalleButton }
            };

            grid.Children.Add(opcionesLayout, 5, 0);

            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }

        private ViewCell CrearViewCellDetalles(PedidoDet detalle)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label numeroLabel = new Label { Text = detalle.numero_pedido.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label productoLabel = new Label { Text = detalle.NombreProducto, HorizontalOptions = LayoutOptions.Center };
            Label cantidadLabel = new Label { Text = detalle.cantidad.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label precioLabel = new Label { Text = detalle.precio_unitario_venta.ToString(), HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(numeroLabel, 0, 0);
            grid.Children.Add(productoLabel, 1, 0);
            grid.Children.Add(cantidadLabel, 2, 0);
            grid.Children.Add(precioLabel, 3, 0);

            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }

    }
}