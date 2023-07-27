using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class PedidoCab
    {
        public int numero_pedido { get; set; }
        public DateTime fecha_pedido { get; set; }
        public string cedula_cliente { get; set; }
        public string id_estado_pedido { get; set; }
        public double subtotal { get; set; }

        public List<PedidoDet> detalles { get; set; }
    }
}
