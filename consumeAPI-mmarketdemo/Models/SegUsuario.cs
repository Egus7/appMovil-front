using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class SegUsuario
    {
        public int id_seg_usuario { get; set; }
        public string codigo { get; set; }
        public string apellidos { get; set; }
        public string nombres { get; set; }
        public string correo { get; set; }
        public string clave { get; set; }
        public bool activo { get; set; }

        // Propiedad calculada para combinar código, nombre y apellidos
        public string Usuario => $"{codigo} - {nombres} {apellidos}";


    }
}
