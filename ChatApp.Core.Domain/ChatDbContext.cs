﻿using ChatApp.Core.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Core.Domain
{
    public class ChatDbContext : DbContext
    {

        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(m => m.Messages)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(u => u.SenderId);

            modelBuilder.Entity<Chat>().HasData(
                    new Chat
                    {
                        ChatId = Guid.NewGuid(),
                        Name = "Global",
                        CreatedAt = DateTime.Now
                    }
                );
        }
    }
}