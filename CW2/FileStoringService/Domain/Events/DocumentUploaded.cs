using System;

namespace FileStoringService.Entities.Events;
public class DocumentUploaded : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid DocumentId { get; }
    public string DocumentName { get; }
    public long DocumentSize { get; }

    public DocumentUploaded(Guid documentId, string documentName, long documentSize)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        DocumentId = documentId;
        DocumentName = documentName;
        DocumentSize = documentSize;
    }
}
