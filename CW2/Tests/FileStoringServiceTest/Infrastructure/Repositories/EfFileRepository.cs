using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileStoringService.Domain.Model.Entities;
using FileStoringService.Infrastructure.Data;
using FileStoringService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FileStoringService.Infrastructure.Repositories
{
    public class EfFileRepositoryTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_Adds_Document()
        {
            using var context = CreateContext();
            var repo = new EfFileRepository(context);
            var doc = Document.Create("file.txt", "/path", "content");

            await repo.AddAsync(doc);
            await context.SaveChangesAsync();

            Assert.Equal(1, context.Files.Count());
            Assert.Equal(doc.Name, context.Files.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Document_When_Exists()
        {
            using var context = CreateContext();
            var doc = Document.Create("file.txt", "/path", "content");
            context.Files.Add(doc);
            context.SaveChanges();

            var repo = new EfFileRepository(context);
            var result = await repo.GetByIdAsync(doc.Id);

            Assert.NotNull(result);
            Assert.Equal(doc.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_Not_Exists()
        {
            using var context = CreateContext();
            var repo = new EfFileRepository(context);

            var result = await repo.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByHashAsync_Returns_Document_When_Exists()
        {
            using var context = CreateContext();
            var doc = Document.Create("file.txt", "/path", "content");
            context.Files.Add(doc);
            context.SaveChanges();

            var repo = new EfFileRepository(context);
            var result = await repo.GetByHashAsync(doc.Hash);

            Assert.NotNull(result);
            Assert.Equal(doc.Hash, result.Hash);
        }

        [Fact]
        public async Task GetByHashAsync_Returns_Null_When_Not_Exists()
        {
            using var context = CreateContext();
            var repo = new EfFileRepository(context);

            var result = await repo.GetByHashAsync("nonexistenthash");

            Assert.Null(result);
        }

        [Fact]
        public async Task ListAsync_Returns_All_Documents()
        {
            using var context = CreateContext();
            var doc1 = Document.Create("a.txt", "/a", "a");
            var doc2 = Document.Create("b.txt", "/b", "b");
            context.Files.AddRange(doc1, doc2);
            context.SaveChanges();

            var repo = new EfFileRepository(context);
            var result = (await repo.ListAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, d => d.Name == "a.txt");
            Assert.Contains(result, d => d.Name == "b.txt");
        }

        [Fact]
        public async Task DeleteAsync_Removes_Document()
        {
            using var context = CreateContext();
            var doc = Document.Create("file.txt", "/path", "content");
            context.Files.Add(doc);
            context.SaveChanges();

            var repo = new EfFileRepository(context);
            await repo.DeleteAsync(doc);
            await context.SaveChangesAsync();

            Assert.Empty(context.Files);
        }
    }
}