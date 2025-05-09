using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Appointment.Infrastructure.Redis
{
    public class RedisPublisher
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisPublisher(IConfiguration configuration)
        {
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { $"{configuration["Redis:Host"]}:{configuration["Redis:Port"]}" },
                Password = configuration["Redis:Password"],
                Ssl = bool.Parse(configuration["Redis:UseSsl"] ?? "true")
            };

            _redis = ConnectionMultiplexer.Connect(configOptions);
            _db = _redis.GetDatabase();
        }

        public async Task PublishAsync(string channel, string message)
        {
            var sub = _redis.GetSubscriber();
            await sub.PublishAsync(channel, message);
        }
    }
}