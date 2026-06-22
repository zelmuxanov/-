using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CarRental.DAL.Data;

namespace CarRental.DAL;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=CarRental;Username=postgres;Password=password;");
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}