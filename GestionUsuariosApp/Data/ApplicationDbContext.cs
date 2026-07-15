using Microsoft.EntityFrameworkCore;
using GestionUsuariosApp.Models;

namespace GestionUsuariosApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
    }
}