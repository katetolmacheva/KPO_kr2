using FileStoringService.Domain.Model.Entities;
using FileStoringService.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
namespace FileStoringService.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Document> Files { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Document>(b =>
        {
            b.ToTable("Files");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired();
            b.Property(x => x.Hash).IsRequired();
            b.Property(x => x.Location).IsRequired();
            b.Property(x => x.UploadDate).IsRequired();
        });
    }
}