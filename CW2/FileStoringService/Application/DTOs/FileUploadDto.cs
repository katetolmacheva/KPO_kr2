using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FileStoringService.Application.DTOs;

public class FileUploadDto
{
    [Required]
    [FromForm(Name = "file")]
    public IFormFile File { get; set; }
}