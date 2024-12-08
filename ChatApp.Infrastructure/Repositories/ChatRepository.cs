﻿using ChatApp.Core.Domain;
using ChatApp.Core.Domain.Exeptions;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<ChatRepository> _logger;

        public ChatRepository(ChatDbContext context, ILogger<ChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Chat> GetChatWithMessages(string chatName, int pageNumber, int pageSize)
        {
            try
            {
                Chat? chat = await GetChat(chatName, pageNumber, pageSize);

                if (chat == null)
                {
                    throw new ChatNotFoundExeption(chatName);
                }

                return chat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while fetching with messages for: {chatName}");
                throw;
            }
        }

        private async Task<Chat?> GetChat(string chatName, int pageNumber, int pageSize)
        {
            return await _context.Chats
                .Where(x => x.Name == chatName)
                .Include(x => x.Messages
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize))
                .ThenInclude(u => u.Sender)
                .FirstOrDefaultAsync();
        }
    }
}