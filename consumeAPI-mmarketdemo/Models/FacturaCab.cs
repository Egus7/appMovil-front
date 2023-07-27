using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.Models
{
    public class FacturaCab
    {
        public string numero_factura { get; set; }
        public string cedula_cliente { get; set; }
        public DateTime fecha_emision { get; set; }
        public double subtotal { get; set; }
        public double base_cero { get; set; }
        public double valor_iva { get; set; }
        public double total { get; set; }
        public bool estado { get; set; }


        public List<FacturaDet> detalles { get; set; }
    }
}
