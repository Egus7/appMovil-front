using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class FacturaDet
    {
        public int numero_factura_det { get; set; }
        public int numero_factura { get; set; }
        public int codigo_producto { get; set; }
        public int cantidad { get; set; }
        public double precio_unitario_venta { get; set; }

        //para almacenar el nombre del producto
        public string nombre_producto { get; set; }
        // Agregar la propiedad subtotal    
        public double subtotal { get; set; }
        // Agregar la propiedad stock
        public int stock { get; set; }

        public string NombreProducto => $"{codigo_producto} - {nombre_producto}";


        // Método para recalcular el subtotal
        public void RecalcularSubtotalFacDet()
        {
            subtotal = cantidad * precio_unitario_venta;
        }
    }
}
