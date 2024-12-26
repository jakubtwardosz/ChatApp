using Confluent.Kafka;

namespace ChatApp.Core.Domain.Interfaces.Producers
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, Message<string, string> message);
    }
}
