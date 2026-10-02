using PortalDeCurriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace PortalDeCurriculos.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Candidato> Candidatos => Set<Candidato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Deve refletir database/01_create_schema.sql
        modelBuilder.Entity<Candidato>(e =>
        {
            e.ToTable("Candidatos");
            e.Property(c => c.NomeCompleto).HasMaxLength(150).IsRequired();
            e.Property(c => c.Email).HasMaxLength(254).IsRequired();
            e.Property(c => c.Telefone).HasMaxLength(30);
            e.Property(c => c.AreaInteresse).HasMaxLength(100);
            e.Property(c => c.ResumoProfissional).HasMaxLength(4000);
            e.Property(c => c.CriadoEm).HasConversion(v => v,
                                           v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });
    }
}
