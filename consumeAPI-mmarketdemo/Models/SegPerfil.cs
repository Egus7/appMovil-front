using consumeAPI_mmarketdemo;
using consumeAPImmarketdemo.API;
using consumeAPImmarketdemo.Paginas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class SegPerfil
    {
        public int id_seg_perfil { get; set; }
        public string nombre_perfil { get; set; }
        public string ruta_acceso { get; set; }
        public int id_seg_modulo { get; set; }

        // Agrega una propiedad para almacenar el nombre del módulo
        public string nombre_modulo { get; set; }
        // Combina el nombre del perfil y el nombre del módulo en una sola propiedad
        public string NombrePerfilModulo => $"{nombre_perfil} - {nombre_modulo}";

    }

}
