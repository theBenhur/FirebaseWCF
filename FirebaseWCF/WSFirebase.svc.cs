using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Security.Cryptography;
using System.Text;
using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseWCF
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el Nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class WSFirebase : Almacen
    {
        static IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "WzbWQpZD5iwpKYjM3BQ3GuyuXyTzgvlQ1CbHvlVR",
            BasePath = "https://tiendaonline-4a110-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client = new FireSharp.FirebaseClient(config);

        private string CreateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
        private string getDataUser(string username)
        {
            var response = client.Get("usuarios/" + username);
            return response.Body.Replace("\"", "");

        }
        private bool existsProduct(string product, string id)
        {
            var response = client.Get("productos/" + product + "/" + id);
            if (response.Body == "null")
                return false;
            return true;
        }
        private bool existsDetails(string document, string id)
        {
            var response = client.Get(document + "/" + id);
            if (response.Body == "null")
                return false;
            return true;
        }
        private string searchProds(string id)
        {
            if (existsDetails("productos/libros", id))
                return "libros";
            if (existsDetails("productos/comics", id))
                return "comics";
            if (existsDetails("productos/mangas", id))
                return "mangas";
            return "";
        }
        private string getMessage(string code)
        {
            var response = client.Get("respuestas/" + code);
            return response.Body;

        }
        public RespuestaSetProd setProducto(string usuario, string contrasenia, string categoria, string producto)
        {
            string password = getDataUser(usuario);
            if (password != "null")
            {
                if (password.Equals(CreateMD5Hash(contrasenia)))
                {
                    try
                    {
                        string cleanProduct = producto.Replace("\n", " ").Replace("\t", " ").Trim();
                        JObject jsonProducto = JObject.Parse(cleanProduct);
                        string root = "";
                        foreach (var e in jsonProducto) { root = e.Key; }
                        if (!existsProduct(categoria, root))
                        {
                            Details prod = new Details()
                            {
                                ISBN = jsonProducto[root]["ISBN"].ToString(),
                                Nombre = jsonProducto[root]["Nombre"].ToString(),
                                Autor = jsonProducto[root]["Autor"].ToString(),
                                Descuento = bool.Parse(jsonProducto[root]["Descuento"].ToString()),
                                Editorial = jsonProducto[root]["Editorial"].ToString(),
                                Precio = float.Parse(jsonProducto[root]["Precio"].ToString())
                            };
                            Producto produc = new Producto()
                            {
                                ISBN = jsonProducto[root]["ISBN"].ToString(),
                                Nombre = jsonProducto[root]["Nombre"].ToString()
                            };

                            client.Set("productos/" + categoria + "/" + root, produc);
                            client.Set("detalles/" + root, prod);
                            return new RespuestaSetProd()
                            {
                                code = 202,
                                message = getMessage("202"),
                                data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                                status = "Success",
                            };
                        }
                        else
                        {
                            return new RespuestaSetProd()
                            {
                                code = 302,
                                message = getMessage("302"),
                                data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                                status = "Error"
                            };
                        }
                    }
                    catch (Newtonsoft.Json.JsonReaderException e)
                    {
                        return new RespuestaSetProd()
                        {
                            code = 303,
                            message = getMessage("303"),
                            status = "Error",
                            data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss")
                        };

                    }
                }
                else
                {
                    return new RespuestaSetProd()
                    {
                        code = 501,
                        message = getMessage("501"),
                        data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                        status = "Error"
                    };
                }
            }
            else
            {
                return new RespuestaSetProd()
                {
                    code = 500,
                    message = getMessage("500"),
                    data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                    status = "Error"
                };
            }

            // {"LIB003":{"Autor":"dave","Descuento":"false","Editorial":"words","ISBN":"LIN003","Nombre":"la dama azul","Precio":"123.4"}}
        }
        public RespuestaUpdateProd updateProducto(string usuario, string contrasenia, string ISBN, string detalles)
        {
            string password = getDataUser(usuario);
            if (password != "null")
            {
                if (password.Equals(CreateMD5Hash(contrasenia)))
                {
                    string cleanDetalles = detalles.Replace("\n", " ").Replace("\t", " ").Trim();
                    JObject jsonProducto = JObject.Parse(cleanDetalles);
                    if (existsDetails("detalles", ISBN))
                    {
                        Details prod = new Details()
                        {
                            ISBN = jsonProducto["ISBN"].ToString(),
                            Nombre = jsonProducto["Nombre"].ToString(),
                            Autor = jsonProducto["Autor"].ToString(),
                            Descuento = bool.Parse(jsonProducto["Descuento"].ToString()),
                            Editorial = jsonProducto["Editorial"].ToString(),
                            Precio = float.Parse(jsonProducto["Precio"].ToString())
                        };
                        Producto produc = new Producto()
                        {
                            ISBN = jsonProducto["ISBN"].ToString(),
                            Nombre = jsonProducto["Nombre"].ToString()
                        };
                        client.Update("detalles/" + ISBN, prod);
                        string categoria = searchProds(ISBN);
                        client.Update("productos/" + categoria + "/" + ISBN, produc);
                        return new RespuestaUpdateProd()
                        {
                            code = 203,
                            message = getMessage("203"),
                            data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                            status = "Success"
                        };
                    }
                    else
                    {
                        return new RespuestaUpdateProd()
                        {
                            code = 304,
                            message = getMessage("304"),
                            data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                            status = "Error"
                        };
                    }
                }
                else
                {
                    return new RespuestaUpdateProd()
                    {
                        code = 501,
                        message = getMessage("501"),
                        data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                        status = "Error"
                    };
                }
            }
            else
            {
                return new RespuestaUpdateProd()
                {
                    code = 500,
                    message = getMessage("500"),
                    data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                    status = "Error"
                };
            }
            // {"Autor":"dave","Descuento":"false","Editorial":"Words","Fecha":"2012","ISBN":"COM003","Precio":"30.40","Nombre":"Spider-Verse"}
        }
        public RespuestaDeleteProd deleteProducto(string usuario, string contrasenia, string ISBN)
        {
            string password = getDataUser(usuario);
            if (password != "null")
            {
                if (password.Equals(CreateMD5Hash(contrasenia)))
                {
                    if (existsDetails("detalles", ISBN))
                    {
                        client.Delete("detalles/" + ISBN);
                        string categoria = searchProds(ISBN);
                        client.Delete("productos/" + categoria +"/"+ ISBN);
                        return new RespuestaDeleteProd()
                        {
                            code = 204,
                            message = getMessage("204"),
                            data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                            status = "Success"
                        };
                    }
                    else
                    {
                        return new RespuestaDeleteProd()
                        {
                            code = 304,
                            message = getMessage("304"),
                            data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                            status = "Error"
                        };
                    }
                }
                else
                {
                    return new RespuestaDeleteProd()
                    {
                        code = 501,
                        message = getMessage("501"),
                        data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                        status = "Error"
                    };
                }
            }
            else
            {
                return new RespuestaDeleteProd()
                {
                    code = 500,
                    message = getMessage("500"),
                    data = DateTime.Now.ToString("yyyy-MM-dd T HH:mm:ss"),
                    status = "Error"
                };
            }
        }
    }
}
