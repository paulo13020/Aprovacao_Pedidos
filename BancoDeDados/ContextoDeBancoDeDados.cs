using Aprovacao_Pedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace Aprovacao_Pedidos.BancoDeDados
{
    public class ContextoDeBancoDeDados : DbContext
    {
        public ContextoDeBancoDeDados(DbContextOptions<ContextoDeBancoDeDados> options) : base(options) { }

        public DbSet<PedidosAprovadosEntidade> pedidosAprovados { get; set; }
        public DbSet<PedidoEntidade> pedidos { get; set; }
        public DbSet<Usuario> usuarios { get; set; }
    }
}
