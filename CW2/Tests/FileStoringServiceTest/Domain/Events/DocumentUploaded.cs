using System;
using FileStoringService.Entities.Events;
using Xunit;

namespace FileStoringService.Domain.Events
{
    public class DocumentUploadedTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var documentId = Guid.NewGuid();
            var documentName = "test.pdf";
            long documentSize = 123456;

            var evt = new DocumentUploaded(documentId, documentName, documentSize);

            Assert.Equal(documentId, evt.DocumentId);
            Assert.Equal(documentName, evt.DocumentName);
            Assert.Equal(documentSize, evt.DocumentSize);
            Assert.NotEqual(Guid.Empty, evt.Id);
            Assert.True((DateTime.UtcNow - evt.OccurredOn).TotalSeconds < 5);
        }

        [Fact]
        public void Constructor_Allows_EmptyDocumentName()
        {
            var documentId = Guid.NewGuid();
            var evt = new DocumentUploaded(documentId, "", 0);

            Assert.Equal("", evt.DocumentName);
            Assert.Equal(0, evt.DocumentSize);
        }

        [Fact]
        public void Constructor_Allows_ZeroAndNegativeSize()
        {
            var documentId = Guid.NewGuid();
            var evtZero = new DocumentUploaded(documentId, "file", 0);
            var evtNegative = new DocumentUploaded(documentId, "file", -1);

            Assert.Equal(0, evtZero.DocumentSize);
            Assert.Equal(-1, evtNegative.DocumentSize);
        }
    }
}