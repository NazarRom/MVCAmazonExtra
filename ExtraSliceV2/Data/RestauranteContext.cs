using ExtraSliceV2.Models;
using Microsoft.EntityFrameworkCore;

namespace ExtraSliceV2.Data
{
    public class RestauranteContext : DbContext
    {
        public RestauranteContext(DbContextOptions<RestauranteContext> options)
            : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Restaurante> Restaurantes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ProductosPedidos> ProductosPedidos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
    }
}
