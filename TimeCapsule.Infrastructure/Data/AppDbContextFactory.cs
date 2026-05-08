using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace TimeCapsule.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.Load();
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var name = Environment.GetEnvironmentVariable("DB_NAME") ?? "timecapsule";
        var user = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres";
        var pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";
        
        var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={pass}";

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
