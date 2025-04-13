﻿using System;

namespace Domain.Common;

public abstract class BaseEntity : IIdProperty
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
