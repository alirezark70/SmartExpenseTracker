using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExpenseTracker.Infra.Persistence.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
    {
        public WriteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();

            // Connection string موقت برای migration
            optionsBuilder.UseSqlServer(
                "Server=localhost,1434;Database=SmartExpenseDB;User Id=sa;Password=VIANb1sdvA3UkJbCjhM5BzK64eO9n3EmDrgSnTCTvP802suNsh;TrustServerCertificate=True;MultipleActiveResultSets=true;",
                b => b.MigrationsAssembly("SmartExpenseTracker.Infra.Persistence")
            );

            return new WriteDbContext(optionsBuilder.Options);
        }
    }
}
