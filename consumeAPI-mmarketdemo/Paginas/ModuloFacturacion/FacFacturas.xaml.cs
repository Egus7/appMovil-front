using consumeAPI_mmarketdemo;
using consumeAPImmarketdemo.API;
using consumeAPImmarketdemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace consumeAPImmarketdemo.Paginas.ModuloFacturacion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FacFacturas : TabbedPage
    {
        private List<Cliente> listaClientes { get; set; }
        private List<Producto> listaProductos { get; set; }

        private ObservableCollection<FacturaDet> detallesFac;

        APIConsume api = new APIConsume();

        public FacFacturas()
        {
            InitializeComponent();

            // Inicializa y llena la lista de clientes
            listaClientes = ObtenerClientes();
            // Inicializa y llena la lista de productos
            listaProductos = ObtenerProductos();
            // Inicializa y llena la lista facturascab
            List<FacturaCab> listaCabeceras = ObtenerFacCabeceras();

            // Crea una nueva instancia de ObservableCollection para la lista de detalles
            detallesFac = new ObservableCollection<FacturaDet>();
            // Establece la lista de detalles como el ItemSource del ListView
            listViewFacDetalles.ItemsSource = detallesFac;


            // Asigna la lista de usuarios al Picker
            pickerCliente.ItemsSource = listaClientes;
            // Asigna la lista de productos Picker
            pickerProducto.ItemsSource = listaProductos;

            // nombre de la propiedad a mostrar en el Picker para el cliente
            pickerCliente.ItemDisplayBinding = new Binding("clienteSel");
            // nombre de la propiedad a mostrar en el Picker para el producto
            pickerProducto.ItemDisplayBinding = new Binding("nombre");

            // Crear la TableSection
            TableSection facturascabSection = new TableSection();

            // Iterar sobre la lista de facturascab y crear una ViewCell para cada
            // facturacab
            foreach (FacturaCab facturascab in listaCabeceras)
            {
                ViewCell viewCell = CrearViewCellCabeceras(facturascab);
                facturascabSection.Add(viewCell);
            }

            // Obtener el TableRoot de la TableView
            var tableRootDet = FacturasCabTableView.Root;

            // Agregar la TableSection al TableRoot
            tableRootDet.Add(facturascabSection);
        }

        private List<FacturaCab> ObtenerFacCabeceras()
        {
            List<FacturaCab> listCabeceras = new List<FacturaCab>();

            try
            {
                using (WebClient wc = new WebClient())
                {

                    wc.Headers.Add("Content-Type", "application/json");

                    var url = $"{api.BaseUrlFac}/facturas";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de pedidos
                        listCabeceras = JsonConvert.DeserializeObject<List<FacturaCab>>(json);
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
                DisplayAlert("Error", "Error al obtener las facturas: " + ex.Message, "Cerrar");
            }

            return listCabeceras;

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

                // Filtrar la lista de productos para mostrar solo aquellos que
                // estén con stock mayor a 0
                productos = productos.Where(p => p.existencia > 0).ToList();

            }

            return productos;

        }

        private void OnAgregarDetalleClicked(object sender, EventArgs e)
        {
            // Validar que se haya seleccionado un producto y se
            // haya ingresado la cantidad
            if ((pickerProducto.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(entryCantidad.Text))
            {
                DisplayAlert("Error", "Por favor, complete todos los campos", "Cerrar");
                return;
            }

            // Obtener el producto seleccionado en el Picker
            var productoSeleccionado = (Producto)pickerProducto.SelectedItem;

            // Convertir la cantidad ingresada a entero
            int cantidadIngresada = Convert.ToInt32(entryCantidad.Text);

            // Verificar si la cantidad ingresada es mayor que el stock disponible
            if (cantidadIngresada > productoSeleccionado.existencia)
            {
                DisplayAlert("Error", "La cantidad ingresada supera el stock disponible", "Cerrar");
                return;
            }

            // Verificar si el producto ya existe en la lista de detalles
            var detalleExistente = detallesFac.FirstOrDefault(d =>
                    d.codigo_producto == productoSeleccionado.codigo_producto);

            if (detalleExistente != null)
            {
                // Si el producto ya existe, aumentar la cantidad y recalcular el subtotal
                int cantidadTotal = detalleExistente.cantidad + cantidadIngresada;

                if (cantidadTotal > productoSeleccionado.existencia)
                {
                    // La cantidad ingresada supera el stock disponible
                    DisplayAlert("Error", "La cantidad ingresada supera el stock disponible", "Cerrar");
                    return;
                }

                detalleExistente.cantidad = cantidadTotal;
                detalleExistente.RecalcularSubtotalFacDet();

                // Notificar a la ListView que los datos han cambiado
                listViewFacDetalles.ItemsSource = null;
                listViewFacDetalles.ItemsSource = detallesFac;
            }
            else
            {
                // Si el producto no existe, crear un nuevo objeto PedidoDet
                // con los datos ingresados por el usuario
                var nuevoDetalle = new Models.FacturaDet
                {
                    codigo_producto = productoSeleccionado.codigo_producto,
                    nombre_producto = productoSeleccionado.nombre,
                    precio_unitario_venta = productoSeleccionado.precio_unitario,
                    cantidad = cantidadIngresada,
                    stock = productoSeleccionado.existencia
                };

                // Calcular el subtotal del nuevo detalle
                nuevoDetalle.RecalcularSubtotalFacDet();

                // Agregar el nuevo detalle a la lista
                detallesFac.Add(nuevoDetalle);

                // Notificar a la ListView que se ha agregado un nuevo detalle
                listViewFacDetalles.ItemsSource = null;
                listViewFacDetalles.ItemsSource = detallesFac;
            }

            // Limpiar los campos de entrada después de agregar el detalle
            pickerProducto.SelectedItem = null;
            entryCantidad.Text = string.Empty;
        }

        private void OnEliminarDetalleClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.CommandParameter
                    is FacturaDet detalleAEliminar)
            {
                detallesFac.Remove(detalleAEliminar);

                // Notificar a la ListView que los datos han cambiado
                listViewFacDetalles.ItemsSource = null;
                listViewFacDetalles.ItemsSource = detallesFac;
            }
        }

        //se actualiza la tabla luego de hacer algun crud (post)
        private void ActualizarTablaFacCab()
        {
            // Obtener la lista de pedidoscab desde alguna fuente de datos
            List<FacturaCab> cabeceras = ObtenerFacCabeceras();

            // Obtener el TableRoot de la TableView
            var tableRoot = FacturasCabTableView.Root;

            // Crear la TableSection
            TableSection cabecerasSection = new TableSection();

            // Obtener la ViewCell de la cabecera por su nombre
            var cabeceraViewCell = cabeceraCab;
            // Agregar la cabecera al TableSection
            cabecerasSection.Add(cabeceraViewCell);

            // Iterar sobre la lista de cabeceras y crear una ViewCell
            // para cada cabecera
            foreach (FacturaCab cabecera in cabeceras)
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

        private void Button_RegistrarFacturaClicked(object sender, EventArgs e)
        {
            // Validar que existan detalles para registrar la factura
            if (detallesFac.Count == 0)
            {
                DisplayAlert("Error", "Debe agregar al menos un detalle al pedido", "Cerrar");
                return;
            }

            // Obtener la fecha del DatePicker
            DateTime fechaEmision = fechaPicker.Date;

            // Obtener el cliente seleccionado en el Picker
            Cliente clienteSeleccionado = (Cliente)pickerCliente.SelectedItem;

            // Crear una lista para almacenar los detalles de la factura
            List<FacturaDet> listaDetalles = detallesFac.ToList();

            // Crear el objeto para enviar al servidor
            var factura = new
            {           
                cedula_cliente = clienteSeleccionado.cedula_cliente,
                fecha_emision = fechaEmision,
                estado = true,
                detalles = listaDetalles
            };

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");

                    // Convertir el objeto pedido a formato JSON
                    var pedidoJson = JsonConvert.SerializeObject(factura);

                    // Establecer la URL del endpoint para registrar el pedido
                    string url = $"{api.BaseUrlFac}/registrarfactura";

                    // Realizar la petición POST al servidor
                    var response = wc.UploadString(url, "POST", pedidoJson);

                    // Deserializar la respuesta JSON
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(response);

                    // Verificar si el pedido se registró correctamente
                    if (responseObject.message == "Factura registrada correctamente")
                    {
                        DisplayAlert("Éxito", "Factura registrado correctamente", "Cerrar");
                        // Limpiar la lista de detalles
                        detallesFac.Clear();
                        // Actualizar la ListView para reflejar los cambios
                        listViewFacDetalles.ItemsSource = null;
                        listViewFacDetalles.ItemsSource = detallesFac;

                        // Actualizar la tabla de cabeceras
                        ActualizarTablaFacCab();
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

        private ViewCell CrearViewCellCabeceras(FacturaCab cabecera)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });          

            Label numeroLabel = new Label { Text = cabecera.numero_factura, HorizontalOptions = LayoutOptions.Center };
            Label cedulaLabel = new Label { Text = cabecera.cedula_cliente, HorizontalOptions = LayoutOptions.Center };
            Label fechaLabel = new Label { Text = cabecera.fecha_emision.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label subtotalLabel = new Label { Text = cabecera.subtotal.ToString(), HorizontalOptions = LayoutOptions.Center };           
            Label ivaLabel = new Label { Text = cabecera.valor_iva.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label totalLabel = new Label { Text = cabecera.total.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label estadoLabel = new Label { Text = cabecera.estado.ToString(), HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(numeroLabel, 0, 0);
            grid.Children.Add(cedulaLabel, 1, 0);
            grid.Children.Add(fechaLabel, 2, 0);
            grid.Children.Add(subtotalLabel, 3, 0);
            grid.Children.Add(ivaLabel, 4, 0);
            grid.Children.Add(totalLabel, 5, 0);
            grid.Children.Add(estadoLabel, 6, 0);

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
                //await tabbedPage.CurrentPage.Navigation.PushAsync(detallePage, true);
            };

            // Crear el StackLayout y agregar los botones
            StackLayout opcionesLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Children = { detalleButton }
            };

            grid.Children.Add(opcionesLayout, 7, 0);

            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }

    }
}