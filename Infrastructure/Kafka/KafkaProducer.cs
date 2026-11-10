using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        public const string CorrelationHeader = "X-Correlation-Id";

        private readonly IProducer<string, string> _producer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KafkaProducer(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                MessageTimeoutMs = 10000,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<T>(string topic, T message, string? key = null)
        {
            var mensaje = new Message<string, string>
            {
                Key = key!,
                Value = JsonSerializer.Serialize(message),
                Headers = new Headers()
            };

            if (_httpContextAccessor.HttpContext?.Items["CorrelationId"] is string correlationId)
                mensaje.Headers.Add(CorrelationHeader, Encoding.UTF8.GetBytes(correlationId));

            await _producer.ProduceAsync(topic, mensaje);
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }
    }
}
