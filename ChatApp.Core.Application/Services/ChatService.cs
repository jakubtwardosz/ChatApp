using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Core.Domain.Models;

using ChatApp.Core.Domain.Consts;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IChatRepository chatRepository, ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
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

        public Task SaveMessage(MessageDto messageDto)
        {
            throw new NotImplementedException();
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
