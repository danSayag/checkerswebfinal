using CheckersWeb.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace CheckersWeb.Data
{
    // UserDbContex class that inherits from DbContext, representing the database context for the application.
    public class UserDbContex : DbContext 
    {
        public DbSet<User> users { get; set; }
        public DbSet<Game> Games { get; set; }

     

        public UserDbContex(DbContextOptions<UserDbContex> options) : base(options){}

        // Configures the model relationships and constraints for the Game entity.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player1)
                .WithMany()
                .HasForeignKey(g => g.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Player2)
                .WithMany()
                .HasForeignKey(g => g.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Winner)
                .WithMany()
                .HasForeignKey(g => g.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Game>()
                .HasOne(g => g.Loser)
                .WithMany()
                .HasForeignKey(g => g.LoserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
