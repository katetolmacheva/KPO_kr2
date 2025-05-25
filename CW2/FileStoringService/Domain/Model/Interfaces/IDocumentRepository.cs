using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileStoringService.Domain.Model.Entities;
using FileStoringService.Entities;

namespace FileStoringService.Domain.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document> AddAsync(Document document);
        Task<Document> GetByIdAsync(Guid id);
        Task<Document> GetByHashAsync(string hash);
        Task<bool> ExistsByHashAsync(string hash);
        Task<IEnumerable<Document>> GetAllAsync();
        Task<bool> DeleteAsync(Guid id);
    }
}
