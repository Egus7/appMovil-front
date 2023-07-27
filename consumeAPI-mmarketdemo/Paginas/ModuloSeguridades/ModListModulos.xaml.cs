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
    public partial class ModListModulos : ContentPage
    {
        public string Token { get; set; }

        APIConsume api = new APIConsume();
        private string endURL = "/apirest/seguridades/modulos";

        public ModListModulos(string token)
        {
            InitializeComponent();
            Token = token;

            //Obtener lista de modulos
            List<SegModulo> modulos = ObtenerModulos();

            // Crear la TableSection
            TableSection modulosSection = new TableSection();

            // Iterar sobre la lista de modulos y crear una ViewCell para cada modulo
            foreach (SegModulo modulo in modulos)
            {
                ViewCell viewCell = CrearViewCellModulo(modulo);
                modulosSection.Add(viewCell);
            }
            // Obtener el TableRoot de la TableView
            var tableRoot = ModsTableView.Root;
            // Agregar la TableSection al TableRoot
            tableRoot.Add(modulosSection);

        }

        private List<SegModulo> ObtenerModulos()
        {
            List<SegModulo> listModulos = new List<SegModulo>();

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");
                    wc.Headers.Add("Access-Token", Token);

                    var url = $"{api.BaseUrl}" + endURL;

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de usuarios
                        listModulos = JsonConvert.DeserializeObject<List<SegModulo>>(json);
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
                DisplayAlert("Error", "Error al obtener los modulos: " + ex.Message, "Cerrar");
            }

            return listModulos;
        }

        private ViewCell CrearViewCellModulo(SegModulo modulo)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label idLabel = new Label { Text = modulo.id_seg_modulo.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label nombreModLabel = new Label { Text = modulo.nombre_modulo, HorizontalOptions = LayoutOptions.Center };

            grid.Children.Add(idLabel, 0, 0);
            grid.Children.Add(nombreModLabel, 1, 0);


            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }

    }
}