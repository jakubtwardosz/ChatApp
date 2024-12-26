using ChatApp.Core.Domain.Consts;
using ChatApp.Core.Domain.Interfaces.Producers;
using ChatApp.Core.Domain.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatApp.Core.Infrastucture.Producers
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IOptions<KafkaOption> kafkaOption, ILogger<KafkaProducer> logger)
        {
            var kafkaSetting = kafkaOption.Value;
            var config = new ConsumerConfig
            {
                GroupId = Group.Message,
                BootstrapServers = kafkaSetting.Url,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
            _logger = logger;
        }

        public async Task ProduceAsync(string topic, Message<string, string> message)
        {
            try
            {
                await _producer.ProduceAsync(topic, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during send message on topic: {topic}");
                throw;
            }
        }
    }
}
