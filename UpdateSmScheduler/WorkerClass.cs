using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateSmScheduler
{
    public class WorkerClass : BackgroundService
    {
        private readonly ILogger<WorkerClass> _logger;
        private Timer _timer;

        public WorkerClass(ILogger<WorkerClass> logger)
        {
            _logger = logger;
        }
        string connectionString = "Data Source=192.168.1.55;Initial Catalog=PDLERP;User ID=BeetaUser;Password=BeetaUser@123;Connection Timeout=120;Trusted_Connection=False;MultipleActiveResultSets=True;Encrypt=false";
        //protected override Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation("WorkerService started at: {time}", DateTime.Now);

        //    // Run task every 1 minute
        //    _timer = new Timer(async state => await RunTask(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        //    return Task.CompletedTask;
        //}
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkerService started at: {time}", DateTime.Now);

            DateTime now = DateTime.Now;

            // Next 12:00 AM
            DateTime nextRun = DateTime.Today.AddDays(1);

            TimeSpan initialDelay = nextRun - now;
            TimeSpan interval = TimeSpan.FromDays(1);

            _timer = new Timer(async state => await RunTask(), null, initialDelay, interval);

            _logger.LogInformation("Next run scheduled at: {time}", nextRun);

            return Task.CompletedTask;
        }
        private async Task RunTask()
        {
            WriteToFile("Task started at " + DateTime.Now);

            try
            {
                ExecuteStoredProcedure();
                _logger.LogInformation("Task executed at {time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running task");
                WriteToFile("Error: " + ex.ToString());
            }

            WriteToFile("Task finished at " + DateTime.Now);
            await Task.CompletedTask;
        }

        private void ExecuteStoredProcedure()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("updt_all_sm", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    string dateString = DateTime.Now.ToString("yyyy-MM-dd");
                    cmd.Parameters.AddWithValue("@rdate", dateString);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void WriteToFile(string message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\LOSDOC";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filepath = Path.Combine(path, "ServiceLog_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt");

            using (StreamWriter sw = File.Exists(filepath) ? File.AppendText(filepath) : File.CreateText(filepath))
            {
                sw.WriteLine(message);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WorkerClass stopping at: {time}", DateTime.Now);
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(cancellationToken);
        }
    }
}
