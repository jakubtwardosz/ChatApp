using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Core.Domain.Models;
using Confluent.Kafka;

using ChatApp.Core.Domain.Consts;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Core.Domain.Interfaces.Producers;
using System.Text.Json;

namespace ChatApp.Core.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatService> _logger;
        private readonly IKafkaProducer _kafkaProducer;

        public ChatService(IChatRepository chatRepository, ILogger<ChatService> logger, IKafkaProducer kafkaProducer)
        {
            _chatRepository = chatRepository;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<ChatDto> GetChatInfo(string chatName)
        {
            var chat = await _chatRepository.GetChatInfo(chatName);

            var chatDto = ConvertToChatDto(chat);

            return chatDto;
        }

        public async Task<ChatDto> GetPaginatedChat(string chatName, int pageNumber, int pageSize)
        {
            var chat = await _chatRepository.GetChatWithMessages(chatName, pageNumber, pageSize);

            var chatDto = ConvertToChatDto(chat);

            return chatDto;
        }

        public async Task SaveMessage(MessageDto messageDto)
        {
            await _kafkaProducer.ProduceAsync(Topic.Message, new Message<string,string>
            {
                Key = messageDto.MessageId.ToString(),
                Value = JsonSerializer.Serialize(messageDto)
            });
        }

        private ChatDto ConvertToChatDto(Chat chat)
        {
            var chatDto = new ChatDto
            {
                Id = chat.ChatId,
                Name = chat.Name,
                Messages = chat.Messages?
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new MessageDto
                    {
                        MessageId = x.MessageId,
                        Sender = x.Sender.Username,
                        MessageText = x.MessageText,
                        CreatedAt = x.CreatedAt
                    })
                    .ToHashSet()
            };  
            return chatDto;
        }
    }
}
