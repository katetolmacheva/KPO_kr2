using System;

namespace FileStoringService.Entities.Events;
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
