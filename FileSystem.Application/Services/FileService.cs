using FileSystem.Core.IRepositories;
using FileSystem.Core.IServices;
using FileSystem.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IFileDataRepository _fileDataRepository;
        private readonly string _queueName;
        private readonly string _hostname;

        public FileService(IFileDataRepository fileDataRepository, IConfiguration configuration)
        {
            _fileDataRepository = fileDataRepository;
            _queueName = configuration["RabbitMQ:QueueName"];
            _hostname = configuration["RabbitMQ:HostName"];
        }

        public async Task ProcessFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var factory = new ConnectionFactory() { HostName = _hostname };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            using var reader = new StreamReader(file.OpenReadStream());

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var body = Encoding.UTF8.GetBytes(line);
                channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

                var fileData = new FileData { Content = line };
                await _fileDataRepository.AddAsync(fileData);
            }

            await _fileDataRepository.SaveAsync();
        }

        public IQueryable<FileData> GetFileData(int pageNo, int pageSize, string content)
        {
            var query = _fileDataRepository.GetFileData();

            if (!string.IsNullOrEmpty(content))
            {
                query = query.Where(x => x.Content.Contains(content));
            }

            var skip = (pageNo - 1) * pageSize;

            return query.Skip(skip).Take(pageSize);
        }
    }
}
