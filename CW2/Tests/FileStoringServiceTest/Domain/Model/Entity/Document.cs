using System;
using System.Text;
using FileStoringService.Domain.Model.Entities;
using Xunit;

namespace FileStoringService.Tests.Domain.Model.Entities
{
    public class DocumentTests
    {
        [Fact]
        public void Create_ValidArguments_CreatesDocumentWithCorrectProperties()
        {
            var name = "file.txt";
            var path = "/storage/file.txt";
            var content = "test content";

            var doc = Document.Create(name, path, content);

            Assert.Equal(name, doc.Name);
            Assert.Equal(path, doc.Location);
            Assert.False(string.IsNullOrWhiteSpace(doc.Hash));
            Assert.NotEqual(Guid.Empty, doc.Id);
            Assert.True((DateTime.UtcNow - doc.UploadDate).TotalSeconds < 5);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidName_ThrowsArgumentException(string invalidName)
        {
            Assert.Throws<ArgumentException>(() => Document.Create(invalidName, "/path", "content"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_InvalidPath_ThrowsArgumentException(string invalidPath)
        {
            Assert.Throws<ArgumentException>(() => Document.Create("name", invalidPath, "content"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateFromContent_InvalidContent_ThrowsArgumentException(string invalidContent)
        {
            Assert.Throws<ArgumentException>(() => Document.CreateFromContent(invalidContent));
        }

        [Fact]
        public void CreateFromContent_ValidContent_ReturnsMd5Hash()
        {
            var content = "abc";
            var expectedHash = "900150983cd24fb0d6963f7d28e17f72";

            var hash = Document.CreateFromContent(content);

            Assert.Equal(expectedHash, hash);
        }

        [Fact]
        public void UpdateHash_UpdatesHashAndUploadDate()
        {
            var doc = Document.Create("file", "/path", "content");
            var oldDate = doc.UploadDate;
            var newHash = "newhash";

            System.Threading.Thread.Sleep(10);
            doc.UpdateHash(newHash);

            Assert.Equal(newHash, doc.Hash);
            Assert.True(doc.UploadDate > oldDate);
        }
    }
}