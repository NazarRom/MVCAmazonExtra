using ApiProyectoExtraSlice.Models;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ExtraSliceV2.Models;
using MVCApiExtraSlice.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;

namespace MVCApiExtraSlice.Services
{
    public class ServiceRestaurante
    {
        private MediaTypeWithQualityHeaderValue Header;
        private BlobServiceClient blobClient;
        private SecretClient secretClient;
        private string UrlApi;

        public ServiceRestaurante(SecretClient secretClient, BlobServiceClient blobClient)
        {
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
            this.secretClient = secretClient;
            this.UrlApi = secretClient.GetSecretAsync("apiurl").Result.Value.Value;
            this.blobClient = blobClient;
        }

        #region BLOBS
        public async Task<string> GetBlobUriAsync(string container, string blobName)
        {
            BlobContainerClient containerClient = this.blobClient.GetBlobContainerClient(container);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            var response = await containerClient.GetPropertiesAsync();
            var properties = response.Value;

            // Will be private if it's None
            if (properties.PublicAccess == Azure.Storage.Blobs.Models.PublicAccessType.None)
            {
                Uri imageUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddSeconds(3600));
                return imageUri.ToString();
            }

            return blobClient.Uri.AbsoluteUri.ToString();
        }
        #endregion

        #region MAIL
        public async Task SendMailAsync
        (string email, string productos, string cantidad)
        {
            KeyVaultSecret keyVaultSecret = await secretClient.GetSecretAsync("mailextraslice");
            // Add services to the container.
            string urlEmail = keyVaultSecret.Value;
            List<Producto> prods = JsonConvert.DeserializeObject<List<Producto>>(productos);
            List<int> cants = JsonConvert.DeserializeObject<List<int>>(cantidad);
            string tablaHtml = "<table>";
            tablaHtml += "<th>Producto</th>";
            tablaHtml += "<th>Descripción</th>";
            tablaHtml += "<th>Precio</th>";
            tablaHtml += "<th>Cantidad</th>";
            tablaHtml += "<th>Total</th>";
            decimal total = 0;
            foreach (Producto pro in prods)
            {
                total += pro.Precio;
            }
            for (var i = 0; i < prods.Count(); i++)
            {
                Producto prod = prods[i];
                int cant = cants[i];

                tablaHtml += "<tr>";
                tablaHtml += "<td>" + prod.Nombre_producto + "</td>";
                tablaHtml += "<td>" + prod.Descripcion + "</td>";
                tablaHtml += "<td>" + prod.Precio + "€" + "</td>";
                tablaHtml += "<td>" + cant + "</td>";
                total += prod.Precio * (cant - 1);

            }
            tablaHtml += "<td>" + total + "€" + "</td>";
            tablaHtml += "</tr>";
            tablaHtml += "</table>";
            var model = new
            {
                email = email,
                asunto = "Pedido Extra Slice",
                mensaje = tablaHtml
            };
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                string json = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(urlEmail, content);
            }
        }

            #endregion

        #region METHODS
            //get token
            public async Task<string> GetTokenAsync
        (string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    UserName = username,
                    Password = password
                };
                string jsonModel = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(jsonModel, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data =
                        await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    string token =
                        jsonObject.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }
        //call async con el token
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        //call async sin el token
        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default;
                }
            }
        }
        #endregion

        #region RESTAURANTES
        //get all restaurantes
        public async Task<List<Restaurante>> GetRestaurantesAsync()
        {
            string request = "api/restaurantes/getallrestaurantes";
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        //get 1 prestaurante
        public async Task<Restaurante> GetOneRestauranteAsync(int id)
        {
            string request = "api/restaurantes/getonerestaurante/" + id;
            Restaurante restaurante = await this.CallApiAsync<Restaurante>(request);
            return restaurante;
        }
        //restaurantes por categoria
        public async Task<List<Restaurante>> RestaurantesByCategoriaAsync(int id)
        {
            string request = "api/restaurantes/restaurantesbycategoria/" + id;
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        //restaurantes por dinero 
        public async Task<List<Restaurante>> RestaurantesByMoneyAsync(int cantidad)
        {
            string request = "api/restaurantes/restaurantesbymoney/" + cantidad;
            List<Restaurante> restaurantes = await this.CallApiAsync<List<Restaurante>>(request);
            return restaurantes;
        }
        public async Task<Restaurante> RestauranteByNameAsync(string name)
        {
            string request = "api/restaurantes/restaurantebyname/" + name;
            Restaurante restaurante = await this.CallApiAsync<Restaurante>(request);
            return restaurante;
        }
        #endregion

        #region PRODUCTOS
        //obtener todos los productos
        public async Task<List<Producto>> GetAllProductosAsync()
        {
            string request = "api/productos/getallproductos";
            List<Producto> productos = await this.CallApiAsync<List<Producto>>(request);
            return productos;
        }
        //get productos fromn session
        public async Task<List<Producto>> GetProductosFromSessionAsync(List<int> ids, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Productos/getproductosfromsession";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                     ("Authorization", "bearer " + token);
                //tenemos que enviar un objeto JSON
                //nos creamos un objeto de la clase Cubo
                //convertimos el objeto a json
                string compra = JsonConvert.SerializeObject(ids);
                //para enviar datos al servicio se utiliza 
                //la clase SytringContent, donde debemos indicar
                //los datos, de ending  y su tipo
                StringContent content = new StringContent(compra, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Producto>>(responseContent);
                }
                else
                {
                    return null;
                }
            };
        }

        //get productos by restaurant
        public async Task<List<Producto>> FindProductosByRestauranIdAsync(int id)
        {
            string request = "api/Productos/FindProductosByRestaurantId/" + id;
            List<Producto> productos = await this.CallApiAsync<List<Producto>>(request);
            return productos;

        }

        //find producto en concreto
        public async Task<Producto> FindProductoAsync(int id)
        {
            string request = "api/productos/findproducto/"+ id;
            Producto producto = await this.CallApiAsync<Producto>(request);
            return producto;
        }
        #endregion

        #region USUARIOS
        public async Task RegisterUserAsync(string nombre, string direccion, string telefono, string email, string pass)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Usuarios/RegisterUser";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                //tenemos que enviar un objeto JSON
                //nos creamos un objeto de la clase Hospital
                UsuarioAux usuario = new UsuarioAux
                {
                   Nombre_cliente = nombre,
                   Direccion = direccion,
                   Telefono = telefono,
                   Email = email,
                   Password = pass
                    
                };
                //convertimos el objeto a json
                string json = JsonConvert.SerializeObject(usuario);
                //para enviar datos al servicio se utiliza 
                //la clase SytringContent, donde debemos indicar
                //los datos, de ending  y su tipo
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        //METODO PROTEGIDO PARA RECUPERAR EL PERFIL
        public async Task<Usuario> GetPerfilUserAsync
            (string token)
        {
            string request = "api/usuarios/perfiluser";
            Usuario empleado = await
                this.CallApiAsync<Usuario>(request, token);
            return empleado;
        }
        #endregion

        #region CARTA
        //finalizar pedido
        public async Task FinalizarPedidoAsync(int idcliente, List<int> idsproducto, List<int> cantidad, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Carta/FinalizarPedido";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                     ("Authorization", "bearer " + token);
                //tenemos que enviar un objeto JSON
                //nos creamos un objeto de la clase Cubo
                //convertimos el objeto a json
                PedidoFinal pedido = new PedidoFinal
                {
                    IdCliente = idcliente,
                    IdsProducto = idsproducto,
                    Cantidad = cantidad
                };
                string json = JsonConvert.SerializeObject(pedido);
                //para enviar datos al servicio se utiliza 
                //la clase SytringContent, donde debemos indicar
                //los datos, de ending  y su tipo
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                
            };
        }

        public async Task<List<Categoria>> GetAllCategoriasAsync()
        {
            string request = "api/Carta/GetAllCategorias";
            List<Categoria> categorias = await this.CallApiAsync<List<Categoria>>(request);
            return categorias;
        }

        #endregion
    }
}

