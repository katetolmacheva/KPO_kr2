using FileAnalysisService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace FileAnalysisService.Infrastructure.Data;

public class TextAnalysisDbContext : DbContext
{
    public TextAnalysisDbContext(DbContextOptions<TextAnalysisDbContext> options)
        : base(options) { }

    public DbSet<FileAnalysisResult> FileAnalysisResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileAnalysisResult>(entity =>
        {
            entity.ToTable("file_analysis_results");

            entity.HasKey(e => e.FileId);

            entity.Property(e => e.FileId)
                .HasColumnName("file_id")
                .IsRequired();

            entity.Property(e => e.FileName)
                .HasColumnName("file_name")
                .IsRequired();

            entity.Property(e => e.ParagraphCount)
                .HasColumnName("paragraph_count")
                .IsRequired();

            entity.Property(e => e.WordCount)
                .HasColumnName("word_count")
                .IsRequired();

            entity.Property(e => e.CharacterCount)
                .HasColumnName("character_count")
                .IsRequired();

            entity.Property(e => e.PlagiarismPercent)
                .HasColumnName("plagiarism_percent")
                .IsRequired();

            entity.Property(e => e.Location)
                .HasColumnName("location")
                .IsRequired();

            entity.Property(e => e.AnalyzedAt)
                .HasColumnName("analyzed_at")
                .IsRequired();
        });
    }
}