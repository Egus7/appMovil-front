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
    public partial class ModSeguridades
        : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public string Token { get; set; }

        public ModSeguridades(string token)
        {
            InitializeComponent();
            Token = token;

            Items = new ObservableCollection<string>
            {
                "Usuarios",
                "Modulos",
                "Asignar modulos",
                "Perfiles"
            };

            MyListView.ItemsSource = Items;
        }


        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            string selectedItem = (string)e.Item;

            switch (selectedItem)
            {
                case "Usuarios":
                    await Navigation.PushAsync(new ModUsuario(Token)); // Reemplaza 'PagApi' con el nombre de tu página de Usuarios
                    break;
                case "Modulos":
                    await Navigation.PushAsync(new ModListModulos(Token)); // Reemplaza 'PagModulos' con el nombre de tu página de Modulos
                    break;
                case "Asignar modulos":
                    await Navigation.PushAsync(new ModAsignaciones(Token)); // Reemplaza 'PagAsignarModulos' con el nombre de tu página de Asignar Modulos
                    break;
                case "Perfiles":
                    await Navigation.PushAsync(new ModPerfiles(Token)); // Reemplaza 'PagPerfiles' con el nombre de tu página de Perfiles
                    break;
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
