using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace FirebaseWCF
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface Almacen
    {
        // TODO: agregue aquí sus operaciones de servicio
        [OperationContract]
        RespuestaSetProd setProducto(string usuario, string contrasenia, string categoria, string producto);
        [OperationContract]
        RespuestaUpdateProd updateProducto(string user, string contrasenia, string isbn, string detalles);
        [OperationContract]
        RespuestaDeleteProd deleteProducto(string user, string contrasenia, string isbn);
    }


    // Utilice un contrato de datos, como se ilustra en el ejemplo siguiente, para agregar tipos compuestos a las operaciones de servicio.
    [DataContract]
    public class RespuestaSetProd
    {
        [DataMember]
        public int code { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public string data { get; set; }
        [DataMember]
        public string status { get; set; }

    }
    [DataContract]
    public class RespuestaUpdateProd
    {
        [DataMember]
        public int code { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public string data { get; set; }
        [DataMember]
        public string status { get; set; }

    }
    [DataContract]
    public class RespuestaDeleteProd
    {
        [DataMember]
        public int code { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public string data { get; set; }
        [DataMember]
        public string status { get; set; }
    }


}
