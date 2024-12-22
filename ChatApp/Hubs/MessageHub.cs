using ChatApp.Core.Domain;
using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Consts;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Security.Claims;

namespace ChatApp.API.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IChatService _chatService;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private readonly string _mainChat = "Global";
        public MessageHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            LogClaims(); // Add this line to log claims
            var username = GetUsername();
            if (!string.IsNullOrEmpty(username))
            {
                _userConnections[username] = Context.ConnectionId;
            }

            await JoinChat(_mainChat);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = GetUsername();
            if (!string.IsNullOrEmpty(username))
            {
                _userConnections.TryRemove(username, out _);
            }
            await LeaveChat(_mainChat);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(string chatName)
        {
            var chat = await _chatService.GetChatInfo(chatName);
            await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());
        }

        public async Task LeaveChat(string chatName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatName);
        }

        public async Task SendMessageToChat(string chatId, string message)
        {
            var username = GetUsername();
            var userId = GetUserId();
            var messageObj = CreateMessage(chatId, message, userId);
            await Clients.Group(chatId).SendAsync(ReceiveHub.Message, username, message);
            await _chatService.SaveMessage(messageObj);
        }

        private void LogClaims()
        {
            foreach (var claim in Context.User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }
        }


        private string GetUsername()
        {
            var userName = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("Empty username");

            return userName;
        }

        private string GetUserId()
        {
            var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimType.Jti)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("Empty userId");
            return userId;
        }

        private MessageDto CreateMessage(string chatId, string message, string userId)
        {
            return new MessageDto
            {
                MessageId = Guid.NewGuid(),
                MessageText = message,
                SenderId = Guid.Parse(userId),
                ChatId = Guid.Parse(chatId),
                CreatedAt = DateTime.Now
            };
        }
    }
}