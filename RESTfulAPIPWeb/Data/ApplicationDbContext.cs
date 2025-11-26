using RESTfulAPIPWeb.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RESTfulAPIPWeb.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {


        public DbSet<categoria> Categorias { get; set; }
        public DbSet<produto> Produtos { get; set; }
        public DbSet<ModoEntrega> ModosEntrega { get; set; }
     
    }
}
