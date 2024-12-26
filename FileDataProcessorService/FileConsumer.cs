using FileSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace FileDataProcessorService
{
    public class FileConsumer : BackgroundService
    {
        private readonly ILogger<FileConsumer> _logger;
        private readonly IConfiguration _configuration;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private int _second = 0;
        private int _count = 0;
        private readonly int _throttleCount;

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public FileConsumer(ILogger<FileConsumer> logger, IConfiguration configuration, IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _contextFactory = contextFactory;
            _throttleCount = _configuration.GetValue<int>("ThrottleCount");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var hostname = _configuration["RabbitMQ:HostName"];
            _queueName = _configuration["RabbitMQ:QueueName"];

            _connectionFactory = new ConnectionFactory
            {
                HostName = hostname,
                Port = 5672,
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"],
                DispatchConsumersAsync = true
            };

            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclarePassive(_queueName);
            _channel.BasicQos(0, 1, false);
            Console.WriteLine($"Queue [{_queueName}] is waiting for messages.");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                int second = DateTime.Now.Second;

                if (second != _second)
                {
                    //Reset count to 0 when second changes
                    _count = 0;
                    _second = second;
                }

                Console.WriteLine($"second {second} count {_count}");

                //Throttle if count greater than 30
                if (_count >= _throttleCount)
                {
                    //Throttle second must be 1000 - current milli second
                    //Because 1000 ms = 1 second
                    var throttleMilliSeconds = 1000 - DateTime.Now.Millisecond;
                    Console.WriteLine($"Throttled for {throttleMilliSeconds} milliseconds");

                    //Wait till next second
                    await Task.Delay(throttleMilliSeconds);

                    //Reset count and second
                    _count = 0;
                    _second = DateTime.Now.Second;
                    Console.WriteLine("Throttle over");
                }

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                await SaveDataToDatabase(message);

                //Increase count
                _count++;
            };

            _channel.BasicConsume(_queueName, autoAck: true, consumer: consumer);

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
            Console.WriteLine("RabbitMQ connection is closed.");
        }

        private async Task SaveDataToDatabase(string message)
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var fileData = new FileData { Content = message };
            dbContext.FileData.Add(fileData);
            await dbContext.SaveChangesAsync();
        }
    }

}
