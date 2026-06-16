using Microsoft.EntityFrameworkCore;

namespace InfoGiovani_Back.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Allegato> Allegati { get; set; }
    public DbSet<Categoria> Categorie { get; set; }
    public DbSet<CategoriaScheda> CategorieSchede { get; set; }
    public DbSet<Citta> Citta { get; set; }
    public DbSet<Ente> Enti { get; set; }
    public DbSet<Province> Province { get; set; }
    public DbSet<Regioni> Regioni { get; set; }
    public DbSet<Ruoli> Ruoli { get; set; }
    public DbSet<Scheda> Schede { get; set; }
    public DbSet<Utente> Utenti { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── ALLEGATO ────────────────────────────────────────────
        modelBuilder.Entity<Allegato>(e =>
        {
            e.HasKey(a => a.IdAllegato);
            e.Property(a => a.IdAllegato).ValueGeneratedOnAdd();
            e.Property(a => a.Nome).HasMaxLength(255).IsRequired();
            e.Property(a => a.Estensione).HasMaxLength(5);
            e.Property(a => a.Url).HasMaxLength(500);

            e.HasCheckConstraint("CHK_allegato_documento_o_url",
                "Documento IS NOT NULL OR Url IS NOT NULL");

            e.HasOne(a => a.Scheda)
             .WithMany(s => s.Allegati)
             .HasForeignKey(a => a.IdScheda)
             .HasConstraintName("FK_allegato_scheda");
        });

        // ── CATEGORIA ───────────────────────────────────────────
        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasKey(c => c.IdCategoria);
            e.Property(c => c.IdCategoria).ValueGeneratedOnAdd();
            e.Property(c => c.Descrizione).HasMaxLength(250);

            e.HasOne(c => c.Parent)
             .WithMany(c => c.SottoCategorie)
             .HasForeignKey(c => c.IdParents)
             .HasConstraintName("FK_categoria_parent")
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── CATEGORIA SCHEDA ────────────────────────────────────
        modelBuilder.Entity<CategoriaScheda>(e =>
        {
            e.ToTable("categorieschede");
            e.HasKey(cs => cs.IdCategoriaScheda);
            e.Property(cs => cs.IdCategoriaScheda).ValueGeneratedOnAdd();

            e.HasIndex(cs => new { cs.IdScheda, cs.IdCategoria }).IsUnique();

            e.HasOne(cs => cs.Categoria)
             .WithMany(c => c.CategorieSchede)
             .HasForeignKey(cs => cs.IdCategoria)
             .HasConstraintName("FK_categorieschede_categoria");

            e.HasOne(cs => cs.Scheda)
             .WithMany(s => s.CategorieSchede)
             .HasForeignKey(cs => cs.IdScheda)
             .HasConstraintName("FK_categorieschede_scheda");
        });

        // ── CITTA ───────────────────────────────────────────────
        modelBuilder.Entity<Citta>(e =>
        {
            e.ToTable("citta");
            e.HasKey(c => c.IdCitta);
            e.Property(c => c.IdCitta).ValueGeneratedNever(); // ISTAT
            e.Property(c => c.NomeCitta).HasMaxLength(250);

            e.HasOne(c => c.Provincia)
             .WithMany(p => p.Citta)
             .HasForeignKey(c => c.IdProvincia)
             .HasConstraintName("FK_citta_province");
        });

        // ── ENTE ────────────────────────────────────────────────
        modelBuilder.Entity<Ente>(e =>
        {
            e.HasKey(en => en.IdEnte);
            e.Property(en => en.IdEnte).ValueGeneratedOnAdd();
            e.Property(en => en.Nome).HasMaxLength(250).IsRequired();
            e.Property(en => en.Telefono1).HasMaxLength(20);
            e.Property(en => en.Telefono2).HasMaxLength(20);
            e.Property(en => en.Fax).HasMaxLength(20);
            e.Property(en => en.Email).HasMaxLength(250);
            e.Property(en => en.Indirizzo).HasMaxLength(250);
            e.Property(en => en.Url).HasMaxLength(250);
            e.Property(en => en.Contatto).HasMaxLength(250);

            e.HasOne(en => en.Citta)
             .WithMany(c => c.Enti)
             .HasForeignKey(en => en.IdCitta)
             .HasConstraintName("FK_ente_citta");

            e.HasOne(en => en.UtenteCreazione)
             .WithMany()
             .HasForeignKey(en => en.IdUtenteCreazione)
             .HasConstraintName("FK_ente_utente_creazione")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(en => en.UtenteModifica)
             .WithMany()
             .HasForeignKey(en => en.IdUtenteModifica)
             .HasConstraintName("FK_ente_utente_modifica")
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PROVINCE ────────────────────────────────────────────
        modelBuilder.Entity<Province>(e =>
        {
            e.HasKey(p => p.IdProvincia);
            e.Property(p => p.IdProvincia).ValueGeneratedNever(); // ISTAT
            e.Property(p => p.SiglaProvincia).HasMaxLength(2);
            e.Property(p => p.NomeProvincia).HasMaxLength(100);

            e.HasOne(p => p.Regione)
             .WithMany(r => r.Province)
             .HasForeignKey(p => p.IdRegione)
             .HasConstraintName("FK_province_regioni");
        });

        // ── REGIONI ─────────────────────────────────────────────
        modelBuilder.Entity<Regioni>(e =>
        {
            e.HasKey(r => r.IdRegione);
            e.Property(r => r.IdRegione).ValueGeneratedNever(); // ISTAT
            e.Property(r => r.NomeRegione).HasMaxLength(100);
        });

        // ── RUOLI ───────────────────────────────────────────────
        modelBuilder.Entity<Ruoli>(e =>
        {
            e.HasKey(r => r.IdRuolo);
            e.Property(r => r.IdRuolo).ValueGeneratedOnAdd();
            e.Property(r => r.NomeRuolo).HasMaxLength(50);

            e.HasOne(r => r.UtenteCreazione)
             .WithMany()
             .HasForeignKey(r => r.IdUtenteCreazione)
             .HasConstraintName("FK_ruoli_utente_creazione")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.UtenteModifica)
             .WithMany()
             .HasForeignKey(r => r.IdUtenteModifica)
             .HasConstraintName("FK_ruoli_utente_modifica")
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SCHEDA ──────────────────────────────────────────────
        modelBuilder.Entity<Scheda>(e =>
        {
            e.HasKey(s => s.IdScheda);
            e.Property(s => s.IdScheda).ValueGeneratedOnAdd();
            e.Property(s => s.CodNumerico).HasMaxLength(50);
            e.Property(s => s.CodAlfabetico).HasMaxLength(50);
            e.Property(s => s.Titolo).HasMaxLength(200).IsRequired();

            e.HasOne(s => s.UtenteCreazione)
             .WithMany()
             .HasForeignKey(s => s.IdUtenteCreazione)
             .HasConstraintName("FK_scheda_utente_creazione")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.UtenteModifica)
             .WithMany()
             .HasForeignKey(s => s.IdUtenteModifica)
             .HasConstraintName("FK_scheda_utente_modifica")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Ente)
             .WithMany(en => en.Schede)
             .HasForeignKey(s => s.IdEnte)
             .HasConstraintName("FK_scheda_ente");
        });

        // ── UTENTE ──────────────────────────────────────────────
        modelBuilder.Entity<Utente>(e =>
        {
            e.HasKey(u => u.IdUtente);
            e.Property(u => u.IdUtente).ValueGeneratedOnAdd();
            e.Property(u => u.Nome).HasMaxLength(250).IsRequired();
            e.Property(u => u.Cognome).HasMaxLength(250);
            e.Property(u => u.Username).HasMaxLength(250).IsRequired();
            e.Property(u => u.Password).HasMaxLength(128).IsRequired();
            e.Property(u => u.RefreshToken).HasMaxLength(88);

            // NomeUtente è calcolato dal DB
            e.Property(u => u.NomeUtente)
             .HasComputedColumnSql("Nome + ' ' + ISNULL(Cognome, '')", stored: true);

            e.HasOne(u => u.Ruolo)
             .WithMany(r => r.Utenti)
             .HasForeignKey(u => u.IdRuolo)
             .HasConstraintName("FK_utente_ruolo")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Utente>()
             .WithMany()
             .HasForeignKey(u => u.IdUtenteCreazione)
             .HasConstraintName("FK_utente_creazione")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<Utente>()
             .WithMany()
             .HasForeignKey(u => u.IdUtenteModifica)
             .HasConstraintName("FK_utente_modifica")
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}