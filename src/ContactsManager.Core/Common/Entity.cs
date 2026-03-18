using System;

namespace ContactsManager.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; }

    protected Entity() { }

    protected Entity(Guid id)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
    }
}
