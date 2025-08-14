using Microsoft.EntityFrameworkCore;
using VoiceConnect.Backend.Models;

namespace VoiceConnect.Backend.Data;

public class VoiceConnectDbContext : DbContext
{
    public VoiceConnectDbContext(DbContextOptions<VoiceConnectDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<CallRequest> CallRequests { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToContainer("Users");
            entity.HasPartitionKey(e => e.Id);
            entity.Property(e => e.FavoriteUserIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });

        // Configure Topic
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToContainer("Topics");
            entity.HasPartitionKey(e => e.Id);
            entity.Ignore(e => e.Author);
        });

        // Configure Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToContainer("Messages");
            entity.HasPartitionKey(e => e.Id);
            entity.Ignore(e => e.Sender);
            entity.Ignore(e => e.Recipient);
        });

        // Configure CallRequest
        modelBuilder.Entity<CallRequest>(entity =>
        {
            entity.ToContainer("CallRequests");
            entity.HasPartitionKey(e => e.Id);
            entity.Ignore(e => e.Topic);
            entity.Ignore(e => e.From);
            entity.Ignore(e => e.To);
        });

        // Configure OtpCode
        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.ToContainer("OtpCodes");
            entity.HasPartitionKey(e => e.Id);
        });
    }
}