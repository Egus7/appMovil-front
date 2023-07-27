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
    public partial class ModPerfiles : ContentPage
    {
        public string Token { get; set; }

        APIConsume api = new APIConsume();
        private string endURL = "/apirest/seguridades/perfiles";

        public ModPerfiles(string token)
        {
            InitializeComponent();
            Token = token;

            // Obtener la lista de perfiles
            List<SegPerfil> perfiles = ObtenerPerfiles();

            // Crear la TableSection
            TableSection perfilesSection = new TableSection();


            // Iterar sobre la lista de perfiles y crear una ViewCell para cada perfil
            foreach (SegPerfil perfil in perfiles)
            {
                ViewCell viewCell = CrearViewCellPerfil(perfil);
                perfilesSection.Add(viewCell);
            }

            // Obtener el TableRoot de la TableView
            var tableRoot = PerfsTableView.Root;
            // Agregar la TableSection al TableRoot
            tableRoot.Add(perfilesSection);

        }

        private List<SegPerfil> ObtenerPerfiles()
        {
            List<SegPerfil> perfiles = new List<SegPerfil>();

            try
            {
                
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/json");
                    wc.Headers.Add("Access-Token", Token);

                    var url = $"{api.BaseUrl}/apirest/seguridades/perfiles";

                    // Realizar solicitud GET a la API
                    var json = wc.DownloadString(url);

                    try
                    {
                        // Deserializar la respuesta JSON en una lista de perfiles
                        perfiles = JsonConvert.DeserializeObject<List<SegPerfil>>(json);

                        // Obtener los nombres de los módulos correspondientes a los perfiles
                        foreach (var perfil in perfiles)
                        {
                            var urlModulo = $"{api.BaseUrl}/apirest/seguridades/modulos/{perfil.id_seg_modulo}";
                            var jsonModulos = wc.DownloadString(urlModulo);
                            var modulos = JsonConvert.DeserializeObject<List<SegModulo>>(jsonModulos);

                            if (modulos.Count > 0)
                            {
                                perfil.nombre_modulo = modulos[0].nombre_modulo;
                            }
                        }
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
                DisplayAlert("Error", "Error al obtener los perfiles: " + ex.Message, "Cerrar");
            }

            return perfiles;
        }




        private ViewCell CrearViewCellPerfil(SegPerfil perfil)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label idLabel = new Label { Text = perfil.id_seg_perfil.ToString(), HorizontalOptions = LayoutOptions.Center };
            Label nombrePerfLabel = new Label { Text = perfil.nombre_perfil, HorizontalOptions = LayoutOptions.Center };
            Label moduloLabel = new Label { Text = perfil.nombre_modulo, HorizontalOptions = LayoutOptions.Center };
    
            grid.Children.Add(idLabel, 0, 0);
            grid.Children.Add(nombrePerfLabel, 1, 0);
            grid.Children.Add(moduloLabel, 2, 0);


            ViewCell viewCell = new ViewCell { View = grid };
            return viewCell;
        }

    }
}