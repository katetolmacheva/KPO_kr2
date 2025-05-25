using System;

namespace FileStoringService.Entities.Events;
public class DocumentExist : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid ExistingDocumentId { get; }
    public string DocumentHash { get; }
    public string AttemptedFileName { get; }

    public DocumentExist(Guid existingDocumentId, string documentHash, string attemptedFileName)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ExistingDocumentId = existingDocumentId;
        DocumentHash = documentHash;
        AttemptedFileName = attemptedFileName;
    }
}
