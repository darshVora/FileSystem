using FileSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FileDataProcessorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
            builder.Services.AddHostedService<FileConsumer>();
            builder.Services.AddLogging();
            builder.Services.AddMemoryCache();

            var host = builder.Build();
            host.Run();
        }
    }
}