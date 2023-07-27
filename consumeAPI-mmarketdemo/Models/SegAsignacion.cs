using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class SegAsignacion
    {
        public int id_seg_asignacion { get; set; }
        public int id_seg_usuario { get; set; }
        public int id_seg_perfil { get; set; }
        //para el nombre_perfil en el list.view
        public string nombre_perfil { get; set; }

        //relaciones con las tablas
        public SegUsuario usuario { get; set; }
        public SegPerfil perfil { get; set; }

    }
}
