using System;

namespace ContactsManager.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity() { }

    protected AuditableEntity(Guid id)
        : base(id) { }

    public DateTimeOffset CreatedAtUtc { get; internal set; }
    public string? CreatedBy { get; internal set; }

    public DateTimeOffset LastModifiedUtc { get; internal set; }
    public string? LastModifiedBy { get; internal set; }
}
