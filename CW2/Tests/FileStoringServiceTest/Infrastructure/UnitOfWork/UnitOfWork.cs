using System;
using System.Threading;
using System.Threading.Tasks;
using FileStoringService.Infrastructure.Data;
using FileStoringService.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FileStoringService.Infrastructure.UnitOfWork
{
    public class EfUnitOfWorkTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task SaveChangesAsync_Calls_DbContext_And_Returns_Result()
        {
            using var context = CreateContext();
            var uow = new EfUnitOfWork(context);

            context.Files.Add(FileStoringService.Domain.Model.Entities.Document.Create("a.txt", "/a", "a"));
            var result = await uow.SaveChangesAsync();

            Assert.Equal(1, result);
        }

        [Fact]
        public void Dispose_Disposes_DbContext()
        {
            var context = CreateContext();
            var uow = new EfUnitOfWork(context);

            uow.Dispose();

            Assert.Throws<ObjectDisposedException>(() => context.Files.Count());
        }
    }
}