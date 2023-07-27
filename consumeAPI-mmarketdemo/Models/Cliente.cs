using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class Cliente
    {
        public string cedula_cliente { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string direccion { get; set; }

        // Propiedad calculada para combinar nombre y apellidos
        public string clienteSel => $"{nombres} {apellidos}";
    }
}
