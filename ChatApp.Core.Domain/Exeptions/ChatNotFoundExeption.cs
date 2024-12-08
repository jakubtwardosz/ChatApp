using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core.Domain.Exeptions
{
    public class ChatNotFoundExeption : Exception
    {
        public ChatNotFoundExeption(string chatName)
            : base($"Chat with name '{chatName}' not found") 
        {
        }
    }
}
