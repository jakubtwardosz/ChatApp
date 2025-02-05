﻿namespace ChatApp.Core.Domain.Dtos
{
    public class MessageDto
    {
        public Guid MessageId { get; set; }
        public string Sender { get; set; }
        public string MessageText { get; set; }
        public Guid ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid SenderId { get; set; }
    }
}
