using System;
using FileStoringService.Domain.Model.Entities;
using FileStoringService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FileStoringService.Infrastructure.Data
{
    public class AppDbContextTests
    {
        private DbContextOptions<AppDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Can_Create_Context_And_Access_DbSet()
        {
            var options = CreateOptions();
            using var context = new AppDbContext(options);

            Assert.NotNull(context.Files);
        }

        [Fact]
        public void Can_Add_And_Retrieve_Document()
        {
            var options = CreateOptions();
            var doc = Document.Create("file.txt", "/path/file.txt", "content");

            using (var context = new AppDbContext(options))
            {
                context.Files.Add(doc);
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var loaded = context.Files.Find(doc.Id);
                Assert.NotNull(loaded);
                Assert.Equal(doc.Name, loaded.Name);
                Assert.Equal(doc.Hash, loaded.Hash);
                Assert.Equal(doc.Location, loaded.Location);
                Assert.Equal(doc.UploadDate, loaded.UploadDate);
            }
        }

        [Fact]
        public void Model_Has_Correct_Configuration()
        {
            var options = CreateOptions();
            using var context = new AppDbContext(options);

            var entity = context.Model.FindEntityType(typeof(Document));
            Assert.NotNull(entity);
            Assert.Equal("Files", entity.GetTableName());
            Assert.True(entity.FindProperty(nameof(Document.Name)).IsNullable == false);
            Assert.True(entity.FindProperty(nameof(Document.Hash)).IsNullable == false);
            Assert.True(entity.FindProperty(nameof(Document.Location)).IsNullable == false);
            Assert.True(entity.FindProperty(nameof(Document.UploadDate)).IsNullable == false);
        }
    }
}