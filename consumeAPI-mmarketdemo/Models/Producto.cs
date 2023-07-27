using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class Producto
    {
        public int codigo_producto { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public double precio_unitario { get; set; }
        public int existencia { get; set; }
        public string tiene_impuesto { get; set; }


    }
}
