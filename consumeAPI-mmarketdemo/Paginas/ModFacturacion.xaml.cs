using consumeAPImmarketdemo.Paginas.ModuloFacturacion;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace consumeAPImmarketdemo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModFacturacion : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public ModFacturacion()
        {
            InitializeComponent();

            Items = new ObservableCollection<string>
            {
                "Clientes",
                "Productos",
                "Pedidos",
                "Ventas"
            };
			
			MyListViewFac.ItemsSource = Items;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            string selectedItem = (string)e.Item;

            switch (selectedItem)
            {
                case "Clientes":
                    await Navigation.PushAsync(new FacClientes()); 
                    break;
                case "Productos":
                    await Navigation.PushAsync(new FacProductos()); 
                    break;
                case "Pedidos":
                    await Navigation.PushAsync(new FacPedidos()); // Reemplaza 'PagAsignarModulos' con el nombre de tu página de Asignar Modulos
                    break;
                case "Ventas":
                    await Navigation.PushAsync(new FacFacturas()); // Reemplaza 'PagPerfiles' con el nombre de tu página de Perfiles
                    break;
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
