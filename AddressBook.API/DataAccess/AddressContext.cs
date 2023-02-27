using AddressBook.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.API.DataAccess
{
    public class AddressContext : DbContext
    {
        public AddressContext(DbContextOptions<AddressContext> options) : base(options)
        {
        }
        public DbSet<Address> Addresses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>().HasData(
                new Address { Id = 1, Street = "Orteliuslaan", HouseNumber = "1000", ZipCode = "3528 BD", City = "Utrecht", Country = "Nederland" },
                new Address { Id = 2, Street = "Claude Debussylaan", HouseNumber = "34", ZipCode = "1082 MD", City = "Amsterdam", Country = "Nederland" },
                new Address { Id = 3, Street = "Mr. Treublaan", HouseNumber = "7", ZipCode = "1097 DP", City = "Amsterdam", Country = "Nederland" }
            );
        }
    }
}
