using System.Text.Json;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ECommerce.Infrastructure.Data;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry, ICurrentUserService currentUserService)
    {
        Entry = entry;
        CurrentUserService = currentUserService;
    }

    public EntityEntry Entry { get; }
    public ICurrentUserService CurrentUserService { get; }
    public string? Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserService.UserId,
            UserName = CurrentUserService.UserName ?? "system",
            Action = Action ?? Entry.State.ToString(),
            EntityName = EntityName,
            EntityId = EntityId,
            OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
            IpAddress = CurrentUserService.IpAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
}
