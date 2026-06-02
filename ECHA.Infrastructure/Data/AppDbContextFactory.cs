using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EconomiaComHistoria.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "server=localhost;database=echa;user=root;password=;",
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new AppDbContext(options);
    }
}
