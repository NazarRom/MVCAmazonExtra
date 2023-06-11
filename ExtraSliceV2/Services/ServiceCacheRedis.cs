using ExtraSliceV2.Models;
using MVCApiExtraSlice.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace MVCApiExtraSlice.Services
{
    public class ServiceCacheRedis
    {
        private IDatabase database;
        private ServiceRestaurante service;

        public ServiceCacheRedis(ServiceRestaurante service)
        {
            this.service = service;
            this.database = HelperCacheMultiplexer.GetConnection.GetDatabase();
        }

        #region FAVORITOS
        //TENDREMOS UN METODO PARA AÑADIR PRODUCTOS A FAVORITOS
        public async void AddProductoFavorito(Producto producto, string token)
        {
            //CACHE REDIS FUNCIONA CON CLAVES/VALUES PARA 
            //TRABAJAR CON CADA USUARIO
            //DICHA CLAVE ES PARA TODOS LOS USUARIOS
            //DEBERIAMOS TENER ALGUN METODO PARA SABER EL USUARIO
            //POR EJEMPLO, UTILIZANDO UN LOGIN
            //PARA ALMACENAR UN VALUE, SE UTILIZA STRING EN FORMATO
            //JSON
            //LO QUE ALMACENAREMOS SERA UNA COLECCION DE PRODUCTOS
            //RECUPERAMOS LOS PRODUCTOS SI YA EXISTEN
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string id = JsonConvert.SerializeObject(usuario.IdUser);
            string jsonProductos = this.database.StringGet(id);
            List<Producto> productosList;
            if (jsonProductos == null)
            {
                //NO HEMOS ALMACENADO FAVORITOS, CREAMOS LA COLECCION
                productosList = new List<Producto>();
            }
            else
            {
                //CONVERTIMOS EL JSON DE PRODUCTOS A COLECCION List<>
                productosList =
                    JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
            }
            //AÑADIMOS EL NUEVO PRODUCTO A LA COLECCION
            productosList.Add(producto);
            //SERIALIZAMOS LA COLECCION A JSON PARA ALMACENARLA EN CACHE
            jsonProductos =
                JsonConvert.SerializeObject(productosList);
            this.database.StringSet(id, jsonProductos);
        }

        //METODO PARA RECUPERAR TODOS LOS FAVORITOS
        public async Task<List<Producto>> GetProductosFavoritos(string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string id = JsonConvert.SerializeObject(usuario.IdUser);
            string jsonProductos = this.database.StringGet(id);
            if (jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<Producto> favoritos =
                    JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                return favoritos;
            }
        }

        //METODO PARA ELIMINAR UN PRODUCTO FAVORITO
        public async void DeleteProductoFavorito(int idproducto, string token)
        {

            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string id = JsonConvert.SerializeObject(usuario.IdUser);

            List<Producto> favoritos = await this.GetProductosFavoritos(token);
            if (favoritos != null)
            {
                //BUSCAMOS EL PRODUCTO A ELIMINAR
                Producto productoDelete =
                    favoritos.FirstOrDefault(z => z.IdProducto == idproducto);
                favoritos.Remove(productoDelete);
                //SI NO TENEMOS FAVORITOS, ELIMINAMOS LA CLAVE DE CACHE REDIS
                if (favoritos.Count == 0)
                {
                    this.database.KeyDelete(id);
                }
                else
                {
                    string jsonProductos =
                        JsonConvert.SerializeObject(favoritos);
                    //SI NO PONEMOS TIEMPO EN EL MOMENTO DE ALMACENAR
                    //ELEMENTOS DENTRO DE CACHE REDIS, EL TIEMPO DE 
                    //ALMACENAMIENTO ES DE 24 HORAS
                    //TAMBIEN PODEMOS INDICARLO DENTRO DEL PORTAL DE AZURE
                    //PODEMOS INDICAR DE FORMA EXPLICITA EL TIEMPO
                    //QUE DESEAMOS ALMACENAR LOS DATOS HASTA QUE SON 
                    //ELIMINADOS DE FORMA AUTOMATICA
                    this.database.StringSet(id, jsonProductos
                        , TimeSpan.FromMinutes(30));
                }
            }
        }
        #endregion

        #region CARRITO
        //TENDREMOS UN METODO PARA AÑADIR PRODUCTOS A FAVORITOS
        public async void AddProductoCarrito(Producto producto, string token)
        {
            //CACHE REDIS FUNCIONA CON CLAVES/VALUES PARA 
            //TRABAJAR CON CADA USUARIO
            //DICHA CLAVE ES PARA TODOS LOS USUARIOS
            //DEBERIAMOS TENER ALGUN METODO PARA SABER EL USUARIO
            //POR EJEMPLO, UTILIZANDO UN LOGIN
            //PARA ALMACENAR UN VALUE, SE UTILIZA STRING EN FORMATO
            //JSON
            //LO QUE ALMACENAREMOS SERA UNA COLECCION DE PRODUCTOS
            //RECUPERAMOS LOS PRODUCTOS SI YA EXISTEN
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);
            string jsonProductos = this.database.StringGet(email);
            List<Producto> productosList;
            if (jsonProductos == null)
            {
                //NO HEMOS ALMACENADO FAVORITOS, CREAMOS LA COLECCION
                productosList = new List<Producto>();
            }
            else
            {
                //CONVERTIMOS EL JSON DE PRODUCTOS A COLECCION List<>
                productosList =
                    JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
            }
            //AÑADIMOS EL NUEVO PRODUCTO A LA COLECCION
            productosList.Add(producto);
            //SERIALIZAMOS LA COLECCION A JSON PARA ALMACENARLA EN CACHE
            jsonProductos =
                JsonConvert.SerializeObject(productosList);
            this.database.StringSet(email, jsonProductos);
        }

        //METODO PARA RECUPERAR TODOS LOS FAVORITOS
        public async Task<List<Producto>> GetProductosCarrito(string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);
            string jsonProductos = this.database.StringGet(email);
            if (jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<Producto> carrito =
                    JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                return carrito;
            }
        }

        //METODO PARA ELIMINAR UN PRODUCTO FAVORITO
        public async void DeleteProductoCarrito(int idproducto, string token)
        {

            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);

            List<Producto> carrito = await this.GetProductosCarrito(token);
            if (carrito != null)
            {
                //BUSCAMOS EL PRODUCTO A ELIMINAR
                Producto productoDelete = carrito.FirstOrDefault(z => z.IdProducto == idproducto);
                carrito.Remove(productoDelete);
                //SI NO TENEMOS FAVORITOS, ELIMINAMOS LA CLAVE DE CACHE REDIS
                if (carrito.Count == 0)
                {
                    this.database.KeyDelete(email);
                }
                else
                {
                    string jsonProductos =
                        JsonConvert.SerializeObject(carrito);
                    //SI NO PONEMOS TIEMPO EN EL MOMENTO DE ALMACENAR
                    //ELEMENTOS DENTRO DE CACHE REDIS, EL TIEMPO DE 
                    //ALMACENAMIENTO ES DE 24 HORAS
                    //TAMBIEN PODEMOS INDICARLO DENTRO DEL PORTAL DE AZURE
                    //PODEMOS INDICAR DE FORMA EXPLICITA EL TIEMPO
                    //QUE DESEAMOS ALMACENAR LOS DATOS HASTA QUE SON 
                    //ELIMINADOS DE FORMA AUTOMATICA
                    this.database.StringSet(email, jsonProductos
                        , TimeSpan.FromMinutes(30));
                }
            }
        }

        public async void DeleteAllCarrito(string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);
            this.database.KeyDelete(email);
        }
        #endregion
    }
}
