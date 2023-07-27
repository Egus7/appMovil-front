using System;
using System.Collections.Generic;
using System.Text;

namespace consumeAPImmarketdemo.API
{
    public class APIConsume
    {
        private string baseUrl = "https://mmarket-apirest.azurewebsites.net/minimarketdemoWeb";
        private string baseUrlFac = "https://facturacion-apirest.azurewebsites.net/facturacionWeb/apirest";

        public string BaseUrl
        {
            get { return baseUrl; }
            set { baseUrl = value; }
        }

        public string BaseUrlFac
        {
            get { return baseUrlFac; }
            set { baseUrlFac = value; }

        }


    }
}
