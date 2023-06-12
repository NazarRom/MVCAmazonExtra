using ExtraSliceV2.Models;
using Microsoft.Extensions.Caching.Distributed;
using MVCApiExtraSlice.Services;
using Newtonsoft.Json;

namespace MVCAmazonExtra.Services
{
    public class ServiceCacheAmazon
    {
        private IDistributedCache cache;
        private ServiceRestaurante service;
        public ServiceCacheAmazon(IDistributedCache cache, ServiceRestaurante service)
        {
            this.cache = cache;
            this.service = service;
        }


        #region FAVORITOS AWS
        public async Task<List<Producto>> GetFavoritosRedisAsync(string token)
        {
            //se supone que podriamos tener coches almacenados
            //mediante una key
            //almacenaremos los coches utilizando json y en una coleccion
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string id = JsonConvert.SerializeObject(usuario.IdUser);

            string jsonProductos = await this.cache.GetStringAsync(id);
            if (jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<Producto> productos = JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                return productos;
            }
        }


        public async Task AddFavoritosRedisAsync(Producto product, string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string id = JsonConvert.SerializeObject(usuario.IdUser);
            //preguntar si existen coches o no todavia
            List<Producto> productos = await this.GetFavoritosRedisAsync(token);
            //si no devueleve nada, es la primera vez que almacenamos algo..
            //y creamos la coleccion
            if (productos == null)
            {
                productos = new List<Producto>();
            }
            //añadimos el nuevo coche favorito
            productos.Add(product);
            //serializamos a json
            string jsonProductos = JsonConvert.SerializeObject(productos);
            //almacenamos con la key de redis
            await this.cache.SetStringAsync(id, jsonProductos, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));

        }

        public async Task DeleteFavoritosRedisAsync(int idproducto, string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string id = JsonConvert.SerializeObject(usuario.IdUser);

            List<Producto> productos = await this.GetFavoritosRedisAsync(token);
            if (productos != null)
            {
                Producto prodEliminar = productos.FirstOrDefault(x => x.IdRestaurante == idproducto);
                productos.Remove(prodEliminar);
                //comprobamos si ya no existen coches favoritos
                if (productos.Count == 0)
                {
                    await this.cache.RemoveAsync(id);
                }
                else
                {
                    //SERIALIZAMOS Y ALMACENAMOS LA COLECCION ACTUALIZADA
                    string jsonProductos =
                        JsonConvert.SerializeObject(productos);
                    await this.cache.SetStringAsync(id
                        , jsonProductos, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
                }
            }

        }
        #endregion


        #region CARRITO AWS
        public async Task<List<Producto>> GetCarritoRedisAsync(string token)
        {
            //se supone que podriamos tener coches almacenados
            //mediante una key
            //almacenaremos los coches utilizando json y en una coleccion
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);

            string jsonProductos = await this.cache.GetStringAsync(email);
            if (jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<Producto> productos = JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                return productos;
            }
        }


        public async Task AddCarritoRedisAsync(Producto product, string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);
            //preguntar si existen coches o no todavia
            List<Producto> productos = await this.GetCarritoRedisAsync(token);
            //si no devueleve nada, es la primera vez que almacenamos algo..
            //y creamos la coleccion
            if (productos == null)
            {
                productos = new List<Producto>();
            }
            //añadimos el nuevo coche favorito
            productos.Add(product);
            //serializamos a json
            string jsonRestaurantes = JsonConvert.SerializeObject(productos);
            //almacenamos con la key de redis
            await this.cache.SetStringAsync(email, jsonRestaurantes, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));

        }

        public async Task DeleteCarritoRedisAsync(int idprod, string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);

            List<Producto> productos = await this.GetCarritoRedisAsync(token);
            if (productos != null)
            {
                Producto prodEliminar = productos.FirstOrDefault(x => x.IdRestaurante == idprod);

                productos.Remove(prodEliminar);
                //comprobamos si ya no existen coches favoritos
                if (productos.Count == 0)
                {
                    await this.cache.RemoveAsync(email);
                }
              else
                {
                    //SERIALIZAMOS Y ALMACENAMOS LA COLECCION ACTUALIZADA
                    string jsonProductos =
                        JsonConvert.SerializeObject(productos);
                    await this.cache.SetStringAsync(email
                        , jsonProductos, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
                }

            }

        }


        public async void DeleteAllCarritoRedisAsync(string token)
        {
            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            string email = JsonConvert.SerializeObject(usuario.Email);
            await this.cache.RemoveAsync(email);
        }
        #endregion
    }
}
