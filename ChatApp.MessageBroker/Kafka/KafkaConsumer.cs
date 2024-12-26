using ChatApp.Core.Domain;
using ChatApp.Core.Domain.Consts;
using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Models;
using ChatApp.Core.Domain.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ChatApp.MessageBroker.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaOption _kafkaOption;
        private readonly ILogger<KafkaConsumer> _logger;

        public KafkaConsumer(IServiceScopeFactory scopeFactory, IOptions<KafkaOption> kafkaOption, ILogger<KafkaConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _kafkaOption = kafkaOption.Value;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsuming(stoppingToken), stoppingToken);
        }

        private async Task StartConsuming(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting Kafka consumer...");

                await ConsumeAsync(Topic.Message, stoppingToken);

                _logger.LogInformation("Kafka consumer stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while consuming messages.");
            }
        }

        private async Task ConsumeAsync(string topic, CancellationToken stoppingToken)
        {
            var config = CreateConsumerConfig();
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            try
            {
                consumer.Subscribe(topic);
                _logger.LogInformation($"Subscribed to topic: {topic}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        // Przetwórz wiadomość
                        await ProcessMessage(consumeResult.Message.Value);

                        // Jeśli wszystko się uda, zacommituj offset
                        consumer.Commit(consumeResult);
                        _logger.LogInformation($"Committed offset for message with key: {consumeResult.Message.Key}");
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error occurred while consuming the message.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while processing the message. The message will not be acknowledged.");
                        // Nie commitujemy offsetu, wiadomość zostanie przetworzona ponownie
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Kafka consuming was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing Kafka messages.");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Kafka consumer closed.");
            }
        }

        private ConsumerConfig CreateConsumerConfig()
        {
            return new ConsumerConfig
            {
                GroupId = Group.Message,
                BootstrapServers = _kafkaOption.Url,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Ręczne commitowanie offsetu
            };
        }

        private async Task ProcessMessage(string messageValue)
        {
            try
            {
                var messageOrder = DeserializeMessage(messageValue);
                var message = CreateMessage(messageOrder);

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

                await SaveMessageToDatabase(dbContext, message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message.");
                throw; // Rzucamy wyjątek, aby wiadomość nie została uznana za przetworzoną
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the message.");
                throw; // Rzucamy wyjątek, aby wiadomość nie została uznana za przetworzoną
            }
        }

        private MessageDto DeserializeMessage(string messageValue)
        {
            _logger.LogInformation("Deserializing message...");

            var messageOrder = JsonConvert.DeserializeObject<MessageDto>(messageValue);
            if (messageOrder == null)
            {
                throw new JsonException("Deserialized message is null.");
            }

            return messageOrder;
        }

        private Message CreateMessage(MessageDto messageOrder)
        {
            _logger.LogInformation($"Creating message object for MessageId: {messageOrder.MessageId}");

            return new Message()
            {
                MessageId = messageOrder.MessageId,
                SenderId = messageOrder.SenderId,
                CreatedAt = messageOrder.CreatedAt,
                MessageText = messageOrder.MessageText,
                ChatId = messageOrder.ChatId,
            };
        }

        private async Task SaveMessageToDatabase(ChatDbContext dbContext, Message message)
        {
            try
            {
                _logger.LogInformation($"Saving message with ID {message.MessageId} to the database.");

                await dbContext.Messages.AddAsync(message);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Message with ID {message.MessageId} saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving message to the database.");
                throw; // Rzucamy wyjątek, aby wiadomość nie została uznana za przetworzoną
            }
        }
    }
}