using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UpdateSmScheduler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Register Windows Service
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "UpdateSmScheduler";
            });

            // Register connection string
            //builder.Services.AddSingleton(sp =>
            //"Data Source=192.168.1.55;Initial Catalog=PDLERP;User ID=BeetaUser;Password=BeetaUser@123;Connection Timeout=120;Trusted_Connection=False;MultipleActiveResultSets=True;Encrypt=false");

            // Register Worker service
            builder.Services.AddHostedService<WorkerClass>();

            var host = builder.Build();
            host.Run();
        }
    }
}
