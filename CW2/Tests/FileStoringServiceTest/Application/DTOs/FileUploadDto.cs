using System.ComponentModel.DataAnnotations;
using FileStoringService.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace FileStoringService.Application.DTOs
{
    public class FileUploadDtoTests
    {
        [Fact]
        public void Can_Set_And_Get_File_Property()
        {
            var mockFile = new Mock<IFormFile>().Object;
            var dto = new FileUploadDto { File = mockFile };

            Assert.Equal(mockFile, dto.File);
        }

        [Fact]
        public void Validation_Fails_When_File_Is_Null()
        {
            var dto = new FileUploadDto { File = null };
            var context = new ValidationContext(dto);
            var results = new System.Collections.Generic.List<ValidationResult>();

            var isValid = Validator.TryValidateObject(dto, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(FileUploadDto.File)));
        }

        [Fact]
        public void Validation_Succeeds_When_File_Is_Provided()
        {
            var mockFile = new Mock<IFormFile>().Object;
            var dto = new FileUploadDto { File = mockFile };
            var context = new ValidationContext(dto);
            var results = new System.Collections.Generic.List<ValidationResult>();

            var isValid = Validator.TryValidateObject(dto, context, results, true);

            Assert.True(isValid);
            Assert.Empty(results);
        }
    }
}