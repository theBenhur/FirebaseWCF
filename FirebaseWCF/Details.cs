using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FirebaseWCF
{
    public class Details
    {
        public string ISBN { get; set; }
        public string Nombre { get; set; }
        public string Autor { get; set; }
        public bool Descuento { get; set; }
        public string Editorial {get;set;}
        public float Precio { get; set; }

    }
}