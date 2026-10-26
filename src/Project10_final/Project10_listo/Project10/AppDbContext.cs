using Microsoft.EntityFrameworkCore;

namespace Project10.Datos
{
    internal class AppDbContext : DbContext
    {
        public DbSet<players> Players => Set<players>();
        public DbSet<sessions> Sessions => Set<sessions>();
        public DbSet<kingdomstate> KingdomState => Set<kingdomstate>();
        public DbSet<decisions> Decisions => Set<decisions>();
        public DbSet<decisionoutcomes> DecisionOutcomes => Set<decisionoutcomes>();
        public DbSet<choicelogs> ChoiceLogs => Set<choicelogs>();
        public DbSet<randomevents> RandomEvents => Set<randomevents>();
        public DbSet<eventlogs> EventLogs => Set<eventlogs>();
        public DbSet<sessionrank> SessionRank => Set<sessionrank>();

        // XAMPP: usuario root, sin contraseña, puerto 3306
        private const string connectionString =
            "Server=127.0.0.1;Port=3306;Database=reino;User ID=root;Password=;";

        private static ServerVersion serverVersion;   // se detecta una sola vez

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (serverVersion == null) serverVersion = ServerVersion.AutoDetect(connectionString);
            optionsBuilder.UseMySql(connectionString, serverVersion);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Los nombres de tabla en la base están en minúscula
            modelBuilder.Entity<players>().ToTable("players");
            modelBuilder.Entity<sessions>().ToTable("sessions");
            modelBuilder.Entity<kingdomstate>().ToTable("kingdomstate");
            modelBuilder.Entity<decisions>().ToTable("decisions");
            modelBuilder.Entity<decisionoutcomes>().ToTable("decisionoutcomes");
            modelBuilder.Entity<choicelogs>().ToTable("choicelogs");
            modelBuilder.Entity<randomevents>().ToTable("randomevents");
            modelBuilder.Entity<eventlogs>().ToTable("eventlogs");
            modelBuilder.Entity<sessionrank>().ToTable("sessionrank");
        }
    }
}