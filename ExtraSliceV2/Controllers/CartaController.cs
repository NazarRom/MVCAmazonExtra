﻿using ApiProyectoExtraSlice.Models;
using ExtraSliceV2.Filters;
using ExtraSliceV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MVCAmazonExtra.Helpers;
using MVCAmazonExtra.Models;
using MVCAmazonExtra.Services;
using MVCApiExtraSlice.Services;
using Newtonsoft.Json;
//using ProyectoNugetExtraSlice.Models;

namespace ExtraSliceV2.Controllers
{
    public class CartaController : Controller
    {
        private ServiceRestaurante service;
        private ServiceCacheAmazon serviceCache;
        private ServiceStorageS3 storageS3;
        private IMemoryCache memoryCache;

        public CartaController(ServiceRestaurante service, IMemoryCache memoryCache, ServiceCacheAmazon serviceCache, ServiceStorageS3 storageS3)
        {
            this.service = service;
            this.memoryCache = memoryCache;
            this.serviceCache = serviceCache;
            this.storageS3 = storageS3;

        }


        #region CATEGORIA
        public async Task<IActionResult> Index()
        {
            List<Categoria> categorias = new();
            categorias = await this.service.GetAllCategoriasAsync();
            foreach (var cat in categorias)
            {
                if (cat.Imagen != null)
                {
                    cat.Imagen = "https://publicfotos.s3.amazonaws.com/" + cat.Imagen;
                }
            }
            return View(categorias);
        }
        #endregion


        #region RESTAURANTE

        public async Task<IActionResult> _RestaurantesPartial()
        {
            List<Restaurante> restaurantes = new List<Restaurante>();
            restaurantes = await this.service.GetRestaurantesAsync();
            foreach (var res in restaurantes)
            {
                if (res.Imagen != null)
                {
                    res.Imagen = "https://publicfotos.s3.amazonaws.com/" + res.Imagen;
                }
            }
            return PartialView("_RestaurantesPartial", restaurantes);
        }

        public async Task<IActionResult> _ResaturanteOnCategoria(int idcategoria)
        {

            List<Restaurante> restaurantes = new List<Restaurante>();
            restaurantes = await this.service.RestaurantesByCategoriaAsync(idcategoria); ;
            foreach (var res in restaurantes)
            {
                if (res.Imagen != null)
                {
                    res.Imagen = "https://publicfotos.s3.amazonaws.com/" + res.Imagen;
                }
            }
            return PartialView("_ResaturanteOnCategoria", restaurantes);
        }

        public async Task<IActionResult> _RestaurantesByDinero(int dinero)
        {

            List<Restaurante> restaurantes = new List<Restaurante>();
            restaurantes = await this.service.RestaurantesByMoneyAsync(dinero);
            foreach (var res in restaurantes)
            {
                if (res.Imagen != null)
                {
                    res.Imagen = "https://publicfotos.s3.amazonaws.com/" + res.Imagen;
                }
            }
            return PartialView("_ResaturanteOnCategoria", restaurantes);
        }

        public async Task<IActionResult> _RestauranteByName(string name)
        {
            List<Restaurante> restaurantes = await this.service.GetRestaurantesAsync();
            //var result = restaurantes.Where(r => r.Nombre_restaurante.Contains(name)).ToList();
            var result = restaurantes.Where(x => x.Nombre_restaurante.StartsWith(name, StringComparison.OrdinalIgnoreCase)).Select(x => x.Nombre_restaurante);
            return Json(result);
        }
        public async Task<IActionResult> _ShowRestauranteByName(string name)
        {
            Restaurante restaurante = await this.service.RestauranteByNameAsync(name);

            if (restaurante.Imagen != null)
            {
                restaurante.Imagen = "https://publicfotos.s3.amazonaws.com/" + restaurante.Imagen;
            }

            return PartialView("_ShowRestauranteByName", restaurante);
        }

        public async Task<IActionResult> Restaurante(int idrestaurante)
        {

            RestauranteProductos restauranteProductos = new RestauranteProductos
            {
                Restaurante = await this.service.GetOneRestauranteAsync(idrestaurante),
                Productos = await this.service.FindProductosByRestauranIdAsync(idrestaurante)
            };

            if (restauranteProductos.Restaurante.Imagen != null)
            {
                restauranteProductos.Restaurante.Imagen = "https://publicfotos.s3.amazonaws.com/" + restauranteProductos.Restaurante.Imagen;
            }

            return View(restauranteProductos);
        }
        #endregion

        
        #region CARRITO CACHE
        [AuthorizeUsuarios]
        public async Task<IActionResult> SeleccionarProducto(int idproducto, int idrestaurante)
        {
            //BUSCAMOS EL PRODUCTO 
            Producto producto = await this.service.FindProductoAsync(idproducto);
            //ALMACENAMOS EL PRODUCTO EN CACHE REDIS
            string token = HttpContext.Session.GetString("TOKEN");
            this.serviceCache.AddCarritoRedisAsync(producto, token);
            return RedirectToAction("Restaurante", new { idrestaurante = idrestaurante });
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> CarritoProductos()
        {
            string token = HttpContext.Session.GetString("TOKEN");
            List<Producto> productosFavoritos = await this.serviceCache.GetCarritoRedisAsync(token);
            if (productosFavoritos == null)
            {
                ViewData["mensaje"] = "No hay productos";
            }
            return View(productosFavoritos);
        }

        [HttpPost]
        [AuthorizeUsuarios]
        public async Task<IActionResult> CarritoProductos(List<int> idproducto, List<int> cantidad)
        {
            string token = HttpContext.Session.GetString("TOKEN");
            List<Producto> productosSession = await this.service.GetProductosFromSessionAsync(idproducto, token);
            string productostring = JsonConvert.SerializeObject(productosSession);

            List<int> prodCantidad = cantidad;
            string prodCanString = JsonConvert.SerializeObject(prodCantidad);

            Usuario usuario = await this.service.GetPerfilUserAsync(token);
            //await this.service.SendMailAsync(usuario.Email, productostring, prodCanString);


            await this.service.FinalizarPedidoAsync(usuario.IdUser, idproducto, cantidad, token);


            string nombrevuelta = "";
            HelperPdf helper = new HelperPdf();
            string ruta = helper.GeneraPdf(productosSession, prodCantidad, usuario.Email, ref nombrevuelta);
            using (FileStream stream = new FileStream(ruta, FileMode.OpenOrCreate))
            {
                await this.storageS3.UploadFileAsync(nombrevuelta, stream);

            }
            List<string> lista = new List<string>();
            lista.Add(nombrevuelta);
            await this.service.SendMailAsync(new ModelLamdbaCorreo() { Asunto = "Factura", Email = usuario.Email, Body = "Su factura pringao", Attachments = lista });

            this.serviceCache.DeleteAllCarritoRedisAsync(token);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteCarrito(int idproducto)
        {
            string token = HttpContext.Session.GetString("TOKEN");
            await this.serviceCache.DeleteCarritoRedisAsync(idproducto, token);
            return RedirectToAction("CarritoProductos");
        }

        #endregion


        #region FAVORITO CACHE
        [AuthorizeUsuarios]
        public async Task<IActionResult> SeleccionarFavorito(int idfavorito)
        {
            //BUSCAMOS EL PRODUCTO DENTRO DEL XML PARA RECUPERARLO
            Producto producto = await this.service.FindProductoAsync(idfavorito);
            //ALMACENAMOS EL PRODUCTO EN CACHE REDIS
            string token = HttpContext.Session.GetString("TOKEN");
            this.serviceCache.AddFavoritosRedisAsync(producto, token);
            ViewData["MENSAJE"] = "Producto almacenado en Favoritos";
            return RedirectToAction("Index");
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> Favoritos()
        {
            string token = HttpContext.Session.GetString("TOKEN");
            List<Producto> productosFavoritos = await this.serviceCache.GetFavoritosRedisAsync(token);
            return View(productosFavoritos);
        }

        public async Task<IActionResult> DeleteFavorito(int idproducto)
        {
            string token = HttpContext.Session.GetString("TOKEN");
            this.serviceCache.DeleteFavoritosRedisAsync(idproducto, token);
            return RedirectToAction("Favoritos");
        }
        #endregion


        #region USER

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UsuarioAux usuario)
        {
            await this.service.RegisterUserAsync(usuario.Nombre_cliente, usuario.Direccion, usuario.Telefono, usuario.Email, usuario.Password);
            return RedirectToAction("Index");
        }

        //miedo
        [AuthorizeUsuarios]
        public async Task<IActionResult> PerfilUsuario()
        {
            string token = HttpContext.Session.GetString("TOKEN");
            Usuario user = await this.service.GetPerfilUserAsync(token);
            return View(user);
        }


        #endregion

    }
}
