using ChatApp.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core.Domain.Interfaces.Repositories
{
    public interface IChatRepository
    {
        Task<Chat> GetChatWithMessages(string chatName, int pageNumber, int pageSize);
    }
}
