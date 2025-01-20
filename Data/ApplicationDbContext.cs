using Microsoft.EntityFrameworkCore;
using BookToFlyAPI.Models;
namespace BookToFlyAPI.Data{
    public class ApplicationDbContext : DbContext{
        public ApplicationDbContext(DbContextOptions <ApplicationDbContext> options):base(options){ }

        public DbSet<User> users{get; set;}
        public DbSet<Admin> admins{get; set;}
        public DbSet<Ticket> tickets{get; set;}
        public DbSet<FlightDetails> flightDetails{get; set;}

    }
}