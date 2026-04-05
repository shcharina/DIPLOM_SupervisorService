using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InternshipManager.Api.Data;

public class SharedDbContextFactory : IDesignTimeDbContextFactory<SharedDbContext>
{
    public SharedDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            
        var optionsBuilder = new DbContextOptionsBuilder<SharedDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("SharedConnection"));
        
        return new SharedDbContext(optionsBuilder.Options);
    }
}

public class SupervisorDbContextFactory : IDesignTimeDbContextFactory<SupervisorDbContext>
{
    public SupervisorDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            
        var optionsBuilder = new DbContextOptionsBuilder<SupervisorDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("SupervisorConnection"));
        
        return new SupervisorDbContext(optionsBuilder.Options);
    }
}