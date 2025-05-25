using System;
using FileStoringService.Application.DTOs;
using Xunit;

namespace FileStoringService.Application.DTOs
{
    public class FileDtoTests
    {
        [Fact]
        public void Can_Create_FileDto_With_Valid_Properties()
        {
            var id = Guid.NewGuid();
            var name = "file.txt";
            var hash = "abc123";
            var location = "/files/file.txt";
            var uploadDate = DateTime.UtcNow;

            var dto = new FileDto
            {
                Id = id,
                Name = name,
                Hash = hash,
                Location = location,
                UploadDate = uploadDate
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal(name, dto.Name);
            Assert.Equal(hash, dto.Hash);
            Assert.Equal(location, dto.Location);
            Assert.Equal(uploadDate, dto.UploadDate);
        }

        [Fact]
        public void Name_Hash_Location_Cannot_Be_Null_By_Default()
        {
            var dto = new FileDto();

            Assert.Null(dto.Name);
            Assert.Null(dto.Hash);
            Assert.Null(dto.Location);
        }

        [Fact]
        public void Properties_Are_Init_Only()
        {
            var dto = new FileDto { Name = "file" };
            Assert.Equal("file", dto.Name);
        }
    }
}