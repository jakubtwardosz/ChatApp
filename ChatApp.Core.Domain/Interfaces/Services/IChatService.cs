using ChatApp.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<ChatDto> GetChatInfo(string chatName);
        Task<ChatDto> GetPaginatedChat(string chatName, int pageNumber, int pageSize);
        Task SaveMessage(MessageDto messageDto);
    }
}
